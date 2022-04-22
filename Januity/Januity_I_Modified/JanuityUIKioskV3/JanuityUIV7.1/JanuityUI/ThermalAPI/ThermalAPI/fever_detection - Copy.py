# -*- coding: utf-8 -*-
"""
Created on Sun May 10 17:18:29 2020
Copyright 2020 Terabee S.A.S

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

Synchronization (main) thread for fever detection
"""
from math import isinf, isnan
from multiprocessing import Process, Event, Queue
from threading import Timer, Thread
from time import sleep
import time
import os

import PIL.Image, PIL.ImageTk
from Evo_Mini import EvoSerialTriggerProcess
from state_machine import FeverScreeningStateMachine, StateMachineEvent
from struct import unpack
import serial.tools.list_ports
from threading import Thread
import threading
import cv2
import numpy as np
import serial
import data_utils
import crcmod.predefined
# Image frame library
from PIL import Image, ImageTk
import tkinter as Tk

# Web socket librarys
from tornado.platform.asyncio import AnyThreadEventLoopPolicy
import tornado.httpserver
import tornado.websocket
import tornado.ioloop
import tornado.web
import socket
from tornado import gen
import asyncio
import random
import traceback
import logging as log
import datetime as dt
import sys

websocketdistance= 0
websockettempreture = 0
wecalled = 0
webclient = []
port = []
testSkipped=False
class FeverDetection(Thread):
    MOVEMENT_RANGE_MIN = 30.0
    MOVEMENT_RANGE_MAX = 60.0
    DETECTION_THRESHOLD = 100.0
    def __init__(self, portnames, settings, stopEvent, callback=None):
        Thread.__init__(self)
        self.stopped = stopEvent
        self.portnames = portnames
        global port
        port = self.portnames
        global facedetected
        facedetected=False
        print("checking",port)
        self.settings = settings
        self.callback = callback
        self.distanceThreshold = 55  # cm
        ### Inter-process variables and events ###
        self.distanceTrigger = Event()
        self.distanceQueue = Queue()
        self.temperatureReady = Event()
        self.temperatureQueue = Queue()
    #temp="N/A"
    def EventCallback(self, event):
        # Builds and returns status string
        #temp="N/A"
        #if(self.currentTemperature) != temp:
        statusStr = "Current event: {}".format(event)
        #if self.currentDistance > 45:
        statusStr += ", Current distance: {}".format(self.currentDistance)
        statusStr += ", Current temperature: {}".format(self.currentTemperature)
        #print(" correct currentTemperature ",self.currentTemperature)
        #global websockettempreture
        #websockettempreture = self.currentTemperature
        self.callback(statusStr, event, self.currentTemperature)
        #elif self.currentDistance < 45:
        #print("Please go back of camera",self.currentDistance)
        #else:
        #print(" ")
    
    def initialize(self):
        # Refresh all flags and variables
        # Minus the state machine update here
        self.temperatureReady.clear()
        self.measuring = False
        self.distanceTrigger.clear()
        self.currentTemperature = "N/A"
        self.currentDistance = "N/A"
        global testSkipped
        testSkipped=False
        global wecalled
        wecalled = 0

        # cascPath = "haarcascade_frontalface_default.xml"
        # global faceCascade
        # faceCascade = cv2.CascadeClassifier(cascPath)
        # log.basicConfig(filename='webcam.log',level=log.INFO)
        
        # global anterior
        # anterior = 0

        # Clear the queues in case there are extra values
        while not self.distanceQueue.empty():
            dump = self.distanceQueue.get()
        while not self.temperatureQueue.empty():
            dump = self.temperatureQueue.get()
        self.evoThermal =  threading.Thread(target=EvoThermal, args=(self.portnames["evo_thermal_port"],
                                     self.settings["unit"],
                                     {'trigger_event': self.distanceTrigger,
                                                       'temperatureReady': self.temperatureReady,
                                                       'temperatureQueue': self.temperatureQueue},
                                                  True))

        self.triggerEvo =  threading.Thread(target=EvoSerialTriggerProcess, args=(self.portnames["evo_mini_port"],
                                                  self.distanceThreshold,
                                                  {'recording_trigger': self.distanceTrigger,
                                                                    'distance_queue': self.distanceQueue},
                                                  True))
        self.triggerEvo.daemon = True
        self.evoThermal.daemon = True

        self.triggerEvo.start()
        self.evoThermal.start()
        thread = Thread(target=startServer,);
        thread.setDaemon(True);
        thread.start();
        time.sleep(5);
        ### State machine init ###
        self.stateMachine = FeverScreeningStateMachine(
            event_callback=self.EventCallback, debug=False)
        self.stateMachine.update(StateMachineEvent.READY)
    def run(self):
        while not self.stopped.wait(0.1):
            ### Check if frame is ready ###
            if self.distanceTrigger.is_set() and not self.measuring:
                self.StartMeasurement()
                Timer(2.0, self.CheckMeasurement).start()
            if not self.distanceQueue.empty():
                self.UpdateDistance()
        self.stop()

    def CheckMeasurement(self):
        if self.temperatureReady.is_set():
            if not self.temperatureQueue.empty():
                while not self.temperatureQueue.empty():
                    temperature = self.temperatureQueue.get()
                self.PrintTemperature(temperature)
                #print("kishore", temperature)
                self.stateMachine.update(StateMachineEvent.MEASURING_END)
        else:
            self.CannotDetect()

    def StartMeasurement(self):
        self.measuring = True
        self.stateMachine.update(StateMachineEvent.MEASURING_START)

    def PrintTemperature(self, temperature):
        self.currentTemperature = temperature
        

    def UpdateDistance(self):
        while not self.distanceQueue.empty():
            distance = 0.0
            distance = self.distanceQueue.get()
        try:
            print(f'Distance {distance}')
            if isinf(distance) or isnan(distance):
                self.SystemRefresh()
            distance = int(distance)
            self.currentDistance = distance
            global websocketdistance
            websocketdistance = self.currentDistance
            if self.measuring:
                if self.MOVEMENT_RANGE_MIN <= distance >= self.MOVEMENT_RANGE_MAX:
                    if str(self.stateMachine.state_machine_current_state()) == "Measuring":
                        self.CannotDetect()
                    elif str(self.stateMachine.state_machine_current_state()) == "Display_result":
                        self.SystemRefresh()
            else:
                if distance <= self.DETECTION_THRESHOLD and str(
                        self.stateMachine.state_machine_current_state()) != "Alignment":
                    self.stateMachine.update(StateMachineEvent.ALIGNMENT)
                elif distance > self.DETECTION_THRESHOLD and str(
                        self.stateMachine.state_machine_current_state()) == "Alignment":
                    self.SystemRefresh()
        except (ValueError, OverflowError):
            return

    def getStatus(self):
        # Builds and returns status string
        statusStr = "Current state: {}".format(
            self.stateMachine.state_machine_current_state())
        statusStr += ", Current distance: {}".format(self.currentDistance)
        statusStr += ", Current temperature: {}".format(
            self.currentTemperature)
        return statusStr

    def SystemRefresh(self):
        # Refresh all flags and variables
        self.temperatureReady.clear()
        self.measuring = False
        self.distanceTrigger.clear()
        self.currentTemperature = "N/A"
        self.currentDistance = "N/A"
        self.stateMachine.update(StateMachineEvent.READY)
        # Clear the queues in case there are extra values
        while not self.distanceQueue.empty():
            dump = self.distanceQueue.get()
        while not self.temperatureQueue.empty():
            dump = self.temperatureQueue.get()

    def CannotDetect(self):
        self.stateMachine.update(StateMachineEvent.MEASURING_FAIL)
        self.currentTemperature = "N/A"
        sleep(1)
        self.SystemRefresh()
    
    def stop(self):
        self.SystemRefresh()
        exit()

class WSHandler(tornado.websocket.WebSocketHandler):
    print('Websocket Started')
    #global port
    def open(self):
        print ('new connection')
        webclient.append(self)
        self.write_message(port)
    def on_message(self, message):
        # print(port) 
        if message == "opencamera":
            global websockettempreture
            websockettempreture = 0
            global video_capture
            video_capture = cv2.VideoCapture(0, cv2.CAP_DSHOW)
            print("Evo Thermal Screen view on via comment")
            global evoscreen
            evoscreen = True
            global testSkipped
            testSkipped=False
        elif message == "closecamera":            
            websockettempreture = 0
            print("Evo Thermal Screen Closed")
            evoscreen = False
        else:
            print("unable to open Evo thermal screen")
    def on_close(self):
        print ('webscoket connection closed')
        webclient.remove(self)
    def check_origin(self, origin):
        return True
    def send_message(self, message):
        for webc in webclient:
            webc.write_message(message);
        return True;

eventLoop = None;
application = tornado.web.Application([
    (r'/ws', WSHandler),
    ])
def startServer():
    global eventLoop;
    try:
        print("Starting server @%s:%d" %("localhost",9872));
        asyncio.set_event_loop(asyncio.new_event_loop());
        eventLoop = tornado.ioloop.IOLoop();
        application.listen(9872)
        eventLoop.start();
    except KeyboardInterrupt:
        print("^C");
    except:
        print("ERR");
        traceback.print_exc();

#evowindow = True
evoscreen = False
testSkipped=False
class EvoThermal(Thread):
    def __init__(self, portname, unit, shared_variables, debug=False):
        Thread.__init__(self)
        ser = serial.Serial(
            port=portname,  # To be adapted if using UART backboard
            baudrate=115200,  # 460800 for UART backboard
            parity=serial.PARITY_NONE,
            stopbits=serial.STOPBITS_ONE,
            bytesize=serial.EIGHTBITS,
            timeout=0.2
        )

        self.port = ser
        self.serial_lock = threading.Lock()
        self.debug = debug
        ### Shared variables ###
        self.trigger_event = shared_variables["trigger_event"]
        self.temperatureReady = shared_variables["temperatureReady"]
        self.temperatureQueue = shared_variables["temperatureQueue"]
        ### CRC functions ###
        self.crc32 = crcmod.predefined.mkPredefinedCrcFun('crc-32-mpeg')
        self.crc8 = crcmod.predefined.mkPredefinedCrcFun('crc-8')
        ### Activate sensor USB output ###
        self.activate_command = (0x00, 0x52, 0x02, 0x01, 0xDF)
        self.deactivate_command = (0x00, 0x52, 0x02, 0x00, 0xD8)
        self.emissivity98_command = (0x00, 0x51, 0x62, 0x30)
        self.send_command(self.activate_command)
        self.send_command(self.emissivity98_command)
        # Temp unit selection
        self.unit = unit
        # Threshold value of the binary thresholding stage
        self.thresh_value = 180
        # The max threshold value each pixel below thresh_value is set to
        self.max_thresh_value = 255
        # Min and max values for contour areas of human face
        self.detection = False
        self.min_cntr_humn_area = 80
        self.max_cntr_humn_area = 400
        # Average of max temperature
        self.avg_list = []
        self.max_loc = []
        self.AvgMax = 0
         
        if self.debug:
            # For text overlay #
            self.upsample_ratio = 8
            self.font = cv2.FONT_HERSHEY_SIMPLEX
            self.bottomLeftCornerOfText = (0, 0)
            self.fontScale = 0.8
            self.fontColor = (255, 255, 255)
            self.lineType = 2
            ### Visualization window ###

            self.activate_visualization = True
            self.window = Tk.Tk()
            #self.window.wm_attributes("-topmost", 1)
            #self.window.overrideredirect(True)
            #issues here button freeze
            #self.window.attributes('-disabled', True)
            self.window.wm_title("Januity")

            ROOT_DIR = os.path.dirname(os.path.abspath(__file__))
            self.window.iconbitmap(ROOT_DIR +'/favicon.ico')
            #self.window.wm_geometry("640x720")
            self.window.resizable (0, 0)
            self.window.overrideredirect(1)
            self.window.call('wm', 'attributes', '.', '-topmost', '1')
            self.window.protocol ('WM_DELETE_WINDOW', (lambda: 'pass') ())
            self.window_width = 640
            self.window_height = 720
            self.canvas_width = 600
            self.canvas_height = 600
            self.canvas2 = Tk.Canvas(self.window, width=self.canvas_width, height=self.canvas_height)
            self.canvas2.pack(padx=10, pady=15)
            self.ws = self.window.winfo_screenwidth()
            self.hs = self.window.winfo_screenheight()
            self.x = (self.ws/2) - (self.window_width/2)
            self.y = (self.hs/2) - (self.window_height/2)
            self.canvas2.pack(side=Tk.TOP)
            self.text2 = Tk.Label(self.window)
            self.photo = ImageTk.PhotoImage("P")
            self.img = self.canvas2.create_image(300, 300, image=self.photo)
            self.text2 = Tk.Label(self.window)
            self.text2.config(height=9, width=70, text='', font=("Helvetica", 12), anchor="center")
            def close_window ():
                global evoscreen
                evoscreen = False
                global testSkipped
                testSkipped=True
            self.frame = Tk.Frame(self.window)
            self.frame.pack()
            # self.button = Tk.Button (self.frame, text = "SkipTest", command = close_window)
            # self.button.pack()
        self.run()

    def SystemRefresh(self):
        # Refresh all flags and variables
        self.temperatureReady.clear()
        self.measuring = False
        self.distanceTrigger.clear()
        self.currentTemperature = "N/A"
        self.currentDistance = "N/A"
        self.stateMachine.update(StateMachineEvent.READY)
        # Clear the queues in case there are extra values
        while not self.distanceQueue.empty():
            dump = self.distanceQueue.get()
        while not self.temperatureQueue.empty():
            dump = self.temperatureQueue.get()

    def get_thermals(self):
        #print("get_thermals")
        i = 0
        got_frame = False  

        while not got_frame:
            print("get loop starting")
            with self.serial_lock:
                i = i + 1
                print(f"get loop {i}")
                ### Polls for header ###
                header = self.port.read(2)
                if len(header) < 2:
                    self.port.flushInput()
                    print("header less 2")
                    break
                header = unpack('H', header)
                if header[0] == 13:
                    ### Header received, now read rest of frame ###
                    data = self.port.read(2068)
                    ### Calculate CRC for frame (except CRC value and header) ###
                    calculatedCRC = self.crc32(data[:2064])
                    data = unpack("H" * 1034, data)
                    receivedCRC = (data[1032] & 0xFFFF) << 16
                    receivedCRC |= data[1033] & 0xFFFF
                    TA = data[1024]
                    data = data[:1024]
                    data = np.reshape(data, (32, 32))
                    ### Compare calculated CRC to received CRC ###
                    if calculatedCRC == receivedCRC:
                        print("Good CRC. Dropping frame ************************")
                        got_frame = True
                    else:
                        print("Bad CRC. Dropping frame ************************************************************************************")                        
                        break

            self.port.flushInput()

        if self.unit == "C":
            ### Data is sent in dK, this converts it to Celsius ###
            try:
                data = (data / 10.0) - 273.15
                TA = (TA / 10.0) - 273.15  # we keep TA always in Celsius
            except:
                print('exception')
                data = 0
                TA = 0
        else:
            ### Data is sent in dK, this converts it to Fahrenheit ###
            data = 9 / 5 * (data / 10.0 - 273.15) + 32

        
        print(f'got_frame {got_frame} TA {TA}')

        return data, TA, got_frame

    def send_command(self, command):
        ### This avoid concurrent writes/reads of serial ###
        with self.serial_lock:
            self.port.write(command)
            ack = self.port.read(1)
            ### This loop discards buffered frames until an ACK header is reached ###
            while ord(ack) != 20:
                ack = self.port.read(1)
            else:
                ack += self.port.read(3)
            ### Check ACK crc8 ###
            crc8 = self.crc8(ack[:3])
            if crc8 == ack[3]:
                ### Check if ACK or NACK ###
                if ack[2] == 0:
                    if self.debug:
                        print("Command acknowledged")
                    return True
                else:
                    if self.debug:
                        print("Command not acknowledged")
                    return False
            else:
                if self.debug:
                    print("Error in ACK checksum")
                return False

    def detect_and_track(self, grayscale_img, heatmap_img):
        global wecalled
        print(f"detect_and_track {wecalled}")
        wecalled = wecalled + 1 
        # Binary thresholding stage
        ret, thresh = cv2.threshold(
            grayscale_img, self.thresh_value, self.max_thresh_value, cv2.THRESH_BINARY)
        # Contour detection stage for openCV 3
        if cv2.__version__[0] == '3':
            _, contours, _ = cv2.findContours(
                thresh, cv2.RETR_LIST, cv2.CHAIN_APPROX_SIMPLE)
        # Contour detection stage for opencv 2 and 4
        else:
            contours, _ = cv2.findContours(
                thresh, cv2.RETR_LIST, cv2.CHAIN_APPROX_SIMPLE)
        # Calculate all the areas  of the detected contours
        areas = [cv2.contourArea(c) for c in contours]
        self.detection = False
        xmin = 0
        ymin = 0
        xmax = 0
        ymax = 0
        for idx, val in enumerate(areas):
            # Human blob filtration stage
            if self.min_cntr_humn_area <= val <= self.max_cntr_humn_area:
                cntr = contours[idx]
                mask = np.zeros_like(grayscale_img)
                cv2.drawContours(mask, cntr, -1, color=255, thickness=-1)
                if self.debug:
                    # Get the highest value pixel and color it
                    _, _, _, max_loc = cv2.minMaxLoc(grayscale_img, mask=mask)
                    self.max_loc.append(max_loc)
                    if len(self.max_loc) >= 15:
                        max_loc = tuple(
                            map(lambda y: sum(y) / len(y), zip(*self.max_loc)))
                        self.max_loc.pop(0)
                    cv2.rectangle(grayscale_img, (int(max_loc[0]), int(max_loc[1])),
                                  (int(max_loc[0]), int(max_loc[1])),
                                  (0, 0, 255), -1)
                    cv2.rectangle(heatmap_img, (int(max_loc[0]), int(max_loc[1])),
                                  (int(max_loc[0]), int(max_loc[1])),
                                  (0, 0, 0), -1)

                    # Calculate text coordinates
                    self.bottomLeftCornerOfText = (self.upsample_ratio * (int(max_loc[0] + 1)),
                                                   self.upsample_ratio * (int(max_loc[1] + 1)))
                # Fitting bounding boxes over our contours of interest (humans)
                x, y, w, h = cv2.boundingRect(cntr)
                # Final bounding box coordinates
                xmin = x
                ymin = y
                xmax = x + w
                ymax = y + h
                self.detection = True

        if self.debug:
            # Human bounding box instances detction stage
            cv2.rectangle(grayscale_img, (xmin, ymin),
                          (xmax, ymax), (0, 0, 255), 1)
            cv2.rectangle(heatmap_img, (xmin, ymin),
                          (xmax, ymax), (0, 0, 0), 1)
            grayscale_img[ymin:ymax + 1, xmin:xmax + 1] = 255
            # Upsample the output detection images if the upsample ration exists
            if self.upsample_ratio is not None:
                grayscale_img = cv2.resize(grayscale_img, (self.upsample_ratio * grayscale_img.shape[1],
                                                           self.upsample_ratio * grayscale_img.shape[0]),
                                           interpolation=cv2.INTER_LINEAR )
                heatmap_img = cv2.cvtColor(heatmap_img, cv2.COLOR_BGR2RGB)
                heatmap_img = cv2.resize(heatmap_img, (self.canvas_width, self.canvas_height), interpolation=cv2.INTER_LINEAR )
            # Write text
            if self.detection:
                cv2.putText(grayscale_img, "{:.1f}".format(self.AvgMax),
                            self.bottomLeftCornerOfText,
                            self.font,
                            self.fontScale,
                            self.fontColor,
                            self.lineType,
                            cv2.LINE_AA)
                cv2.putText(heatmap_img, "{:.1f}".format(self.AvgMax),
                            self.bottomLeftCornerOfText,
                            self.font,
                            self.fontScale,
                            self.fontColor,
                            self.lineType,
                            cv2.LINE_AA)
                global websockettempreture
                websockettempreture = self.AvgMax
        return grayscale_img, heatmap_img, [xmin, ymin, xmax, ymax]

    def run(self):
        while True:
            print("run ----------")
            frame, TAA, valuePass = self.get_thermals()
            print(f'get thermal result {valuePass}')
            if(not valuePass):
                continue
            grayscale_img = data_utils.scale(frame)
            heatmap_img = data_utils.decode_as_heatmap(grayscale_img)
            grayscale_img, heatmap_img, [xmin, ymin, xmax, ymax] = self.detect_and_track(
                grayscale_img, heatmap_img)
                # Capture frame-by-frame


            # if anterior != len(faces):
            #     anterior = len(faces)
            #     log.info("faces: "+str(len(faces))+" at "+str(dt.datetime.now()))

            if evoscreen:
                print ("Test Starting")
                self.photo = ImageTk.PhotoImage(Image.fromarray(heatmap_img))
                ### Show images ###
            #     ret, frameCam = video_capture.read()
            #     gray = cv2.cvtColor(frameCam, cv2.COLOR_BGR2RGB)
            #     faces = faceCascade.detectMultiScale(
            #         gray,
            #         scaleFactor=1.1,
            #         minNeighbors=5,
            #         minSize=(30, 30)
            #     )
            #     #print ("Webcam Started")
            #     facedetected=False
            # # # Draw a rectangle around the faces
            #     for (x, y, w, h) in faces:
            #         facedetected=True                    
            #         cv2.rectangle(frameCam, (x, y), (x+w, y+h), (0, 255, 0), 2)
            #         cv2.putText(frameCam, str(round(websockettempreture,2)), (x, y-10), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 255, 0), 2)
            # #     cv2.imshow('Video', frame)
            #     self.photo = ImageTk.PhotoImage(image = PIL.Image.fromarray(cv2.cvtColor(frameCam, cv2.COLOR_BGR2RGB)))
                #print(f'Face Detected {facedetected}')
                self.canvas2.itemconfig(self.img, image=self.photo)
                self.text2.pack(side=Tk.BOTTOM)
                if (websocketdistance< 45):
                    self.text2.config(text="Move behind "+"\n\n Please stay still between 45-55 cms of distance from the camera.\n You are "+str(websocketdistance)+" cms away" )
                elif((websocketdistance>= 45) and (websocketdistance<= 55)):
                    self.text2.config(text="Please stay still"+"\n\n Please stay still between 45-55 cms of distance from the camera.\n You are "+str(websocketdistance)+" cms away" )
                elif(websocketdistance> 55):
                    self.text2.config(text="Move closer to the camera "+"\n\n Please stay still between 45-55 cms of distance from the camera.\n You are "+str(websocketdistance)+" cms away" )
                global thread, data;
                if (facedetected):
                    try:
                        #time.sleep(0.125);
                        data = ("TempValue,"+str(websockettempreture) +" DistanceValue,"+ str(websocketdistance) )
                        render = "client.html"
                        if eventLoop is not None:
                            eventLoop.add_callback(WSHandler.send_message, render, str(data));
                    except:
                        print("Err");
                #print ("Thermal Update")
                self.window.update()
                self.window.geometry('%dx%d+%d+%d' % (self.window_width, self.window_height, self.x, self.y))
                self.window.deiconify()
                cv2.waitKey(1)
            else:
                cv2.waitKey(0)
                # try:
                #     video_capture.release()
                # except:
                #     print("")
                # if(testSkipped):
                # 	testSkipped=False
                # 	data = ("TemperatureSkipped Skipped Test")
                # 	render = "client.html"
                # 	if eventLoop is not None:
                # 		eventLoop.add_callback(WSHandler.send_message, render, str(data));
                self.window.withdraw()
            if self.trigger_event.is_set() and self.detection:
                ### Add maximum temperature found in rectangle to average ###
                self.avg_list.append(frame[ymin:ymax + 1, xmin:xmax + 1].max())
                ### Gather frames for about 2 seconds ###
                if len(self.avg_list) >= 14:
                    self.AvgMax = sum(self.avg_list) / len(self.avg_list)
                    if self.debug:
                        print("Detected temperature : {:.1f}°{}".format(
                            self.AvgMax, self.unit))
                    ### Clear list ###
                    self.avg_list = []
                    if not self.temperatureReady.is_set():
                        self.temperatureQueue.put(self.AvgMax)
                        self.temperatureReady.set()
            if not self.trigger_event.is_set():
                ### Clear list ###
                self.avg_list = []
                self.AvgMax = 0.0
            

    def stop(self):
        ### Deactivate USB VCP output and close port ###
        self.send_command(self.deactivate_command)
        self.port.close()