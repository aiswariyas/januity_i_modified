# -*- coding: utf-8 -*-
"""
Created on Fri Jun 12 07:33:49 2020
Terabee SAS (c) 2020

"""
from multiprocessing import Event
from time import sleep

from API_events import APIevent
from fever_detection import FeverDetection
import serial.tools.list_ports
import serial


def callback(string, event, temperature):
    #print("In main thread callback function. Input: ", string)
    if event == APIevent.RESULT_READY:
        print("Current temperature", temperature)



if __name__ == '__main__':
    stopEvent = Event()
    ports = list(serial.tools.list_ports.comports())
    evo_thermal_port = None
    evo_mini_port = None

    for p  in ports:
        if ":5740" in p[2]:
            activate_command = (0x54, 0x45, 0x53, 0x54)
            ser = serial.Serial(port=p[0],baudrate = 115200,parity=serial.PARITY_NONE,stopbits=serial.STOPBITS_ONE,bytesize=serial.EIGHTBITS,timeout=0.2)
                        
            ser.write(activate_command)
                    ### This loop discards buffered frames until an ACK header is reached ###
            while True:
                ack = ser.readline()
                try:
                	print(f"ack {ack.decode('utf-8')}")
                	checkvalue=ack.decode('utf-8')
                	if 'Thermal' in checkvalue:
                		evo_thermal_port=p[0]
                		print(f"Thermal Port is {evo_thermal_port}")
                		ser.close()
                		break
                	elif 'Mini' in checkvalue:
                		evo_mini_port=p[0]
                		print(f"Mini Port is {evo_mini_port}")
                		ser.close()
                		break
                except Exception as inst:
                	print(f'Exception {inst}')
            
    print(f"Terabee Ports Are Thermal: {evo_thermal_port} Mini: {evo_mini_port}")

    MyDetector = FeverDetection(portnames={'evo_thermal_port': evo_thermal_port,  # modify this accordingly
                                           'evo_mini_port': evo_mini_port},  # modify this accordingly
                                settings={"unit": "C"}, stopEvent=stopEvent,
                                callback=callback)    
    MyDetector.initialize()
    MyDetector.getStatus()
    MyDetector.start()

