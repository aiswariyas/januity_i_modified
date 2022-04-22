import cv2
import sys
import logging as log
import datetime as dt
from time import sleep
from PIL import Image, ImageTk
import tkinter as Tk
import time
import PIL.Image, PIL.ImageTk

cascPath = "haarcascade_frontalface_default.xml"
faceCascade = cv2.CascadeClassifier(cascPath)
log.basicConfig(filename='webcam.log',level=log.INFO)

video_capture = cv2.VideoCapture(0)
anterior = 0

upsample_ratio = 8
font = cv2.FONT_HERSHEY_SIMPLEX
bottomLeftCornerOfText = (0, 0)
fontScale = 0.8
fontColor = (255, 255, 255)
lineType = 2
            ### Visualization window ###

activate_visualization = False
window = Tk.Tk()
window.resizable (0, 0)
window.overrideredirect(1)
window.call('wm', 'attributes', '.', '-topmost', '1')
window.protocol ('WM_DELETE_WINDOW', (lambda: 'pass') ())
window_width = 640
window_height = 720
canvas_width = 600
canvas_height = 600
canvas2 = Tk.Canvas(window, width=canvas_width, height=canvas_height)
canvas2.pack(padx=10, pady=15)
ws = window.winfo_screenwidth()
hs = window.winfo_screenheight()
x = (ws/2) - (window_width/2)
y = (hs/2) - (window_height/2)
canvas2.pack(side=Tk.TOP)
text2 = Tk.Label(window)
photo = ImageTk.PhotoImage("P")

text2 = Tk.Label(window)
text2.config(height=9, width=70, text='', font=("Helvetica", 12), anchor="center")

while True:
    if not video_capture.isOpened():
        print('Unable to load camera.')
        sleep(5)
        pass

    # Capture frame-by-frame
    ret, frame = video_capture.read()

    gray = cv2.cvtColor(frame, cv2.COLOR_RGB2GRAY)

    faces = faceCascade.detectMultiScale(
        gray,
        scaleFactor=1.1,
        minNeighbors=5,
        minSize=(30, 30)
    )

    # Draw a rectangle around the faces
    for (x, y, w, h) in faces:
        cv2.rectangle(frame, (x, y), (x+w, y+h), (0, 255, 0), 2)

    if anterior != len(faces):
        anterior = len(faces)
        log.info("faces: "+str(len(faces))+" at "+str(dt.datetime.now()))

    photo = ImageTk.PhotoImage(image = PIL.Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)))
    
    print('Face Detected')
    canvas2.create_image(300, 300, image=photo)
    #canvas2.itemconfig(img, image=photo)
    text2.pack(side=Tk.BOTTOM)
    text2.config(text="Move closer to the camera "+"\n\n Please stay still between 45-55 cms of distance from the camera.\n You are 10 cms away" )

    print ("Thermal Update")
    window.update()
    window.geometry('%dx%d+%d+%d' % (window_width, window_height, x, y))
    window.deiconify()
    cv2.imshow('Video', frame)
# When everything is done, release the capture
video_capture.release()
cv2.destroyAllWindows()
