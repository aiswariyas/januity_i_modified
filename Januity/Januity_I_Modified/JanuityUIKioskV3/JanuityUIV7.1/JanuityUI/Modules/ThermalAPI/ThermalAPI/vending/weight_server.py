from __future__ import division
from _thread import *
import mysql.connector
from mysql.connector import Error
from mysql.connector import errorcode
import tornado.ioloop
import tornado.web
import tornado.websocket
import tornado.template
import tornado.httpclient
import time
import subprocess
import os
import sys
import serial
from pykeyboard import PyKeyboard
from threading import Thread
from tornado.platform.asyncio import AnyThreadEventLoopPolicy
import threading
import tornado.ioloop
import tornado.web
from tornado import gen
import asyncio
import socket
import urllib.request
import random 

machine_id = 2004 #machine ID
print ('Machine ID')
print (machine_id)
os.system("sudo xhost +")
os.system("sudo chmod a+rw /dev/ttyACM0") 
ser=serial.Serial('/dev/ttyACM0',9600)
print(ser.name)
print ('Serial Port Connected')

msg=''


def func1():    
    class MainHandler(tornado.web.RequestHandler):
        def get(self):
            loader = tornado.template.Loader(".")
            self.write(loader.load("index.html").generate())
    
    class WSHandler(tornado.websocket.WebSocketHandler):
        result=[1,2,3]
        self2=None
        def ecg(y):
            global result
            global payment

        def check_origin(self, origin):
            return True
        def open(self):
            print ('.')
            self.write_message(".")
        def on_message(self, y):
            global result
            result = y
            self2=self
            print (result)
            g=self.ecg();
        
            try:
                self.write_message(g)
            except Exception as e:
                print("Error: " + str(e))
        def on_close(self):
            print ('connection closed...')

    asyncio.set_event_loop(asyncio.new_event_loop())
    application = tornado.web.Application([
    (r'/ws', WSHandler),
    (r'/', MainHandler),
    (r"/(.*)", tornado.web.StaticFileHandler, {"path": "./resources"}),
    ])
    if __name__ == "__main__":
       application.listen(9071)
       tornado.ioloop.IOLoop.instance().start()


  
def func2():
    
    print ('Serial Port reading started:')
    
    while 1:      
       msg = ser.inWaiting()
       msg = ser.readline().decode('utf-8')
       if msg != '':
         print(msg); my_function()
	
         

def my_function():
  
    lucky_number = random.randint (1,80)
    reading = {
    1:"You will find a bushel of money.",
    2: "Your smile will tell you what makes you feel good.",
    3: "You are going to have some new clothes.",
    4: "Your family is young, gifted and attractive.",
    5: "There is a true and sincere friendship between you both.",
    6: "A smiling stranger will bring you some troubling news.",
    7: "Face facts with dignity.",
    8: "You are magnetic in your bearing.",
    9: "You are free to invent your life.",
    10: "Good sense is the master of human life.",
    11: "If winter comes, can spring be far behind.?",
    12: "Change is certain, be accepting.",
    13: "If you don't have time to live your life now, when will you?",
    14: "Ignorance never settles a question.",
    15: "The best year-round temperature is a warm heart and a cool head.",
    16: "Avert misunderstanding by calm, poise, and balance.",
    17: "Simplicity and clarity should be your theme in dress.",
    18: "You have a potential urge and the ability for accomplishment.",
    19: "Do you believe? Endurance and persistence will be rewarded.",
    20: "Good Luck bestows upon you. You will get what your heart desires.",
    21: "Pat yourself on the back for creating an opportunity.",
    22: "It could be better, but it's good enough.",
    23: "You will find a thing. It may be important.",
    24: "The calling that has sounded will not be the lasting call.",
    25: "In youth and beauty, wisdom is rare.",
    26: "The sage acts by doing nothing.",
    27: "Move carefully, as when crossing a river in winter.",
    28: "When tempers flare up in the family, too great severity brings remorse.",
    29: "The fortune you seek is in another cookie.",
    30: "A closed mouth gathers no feet.",
    31: "A conclusion is simply the place where you got tired of thinking.",
    32: "A cynic is only a frustrated optimist.",
    33: "A foolish man listens to his heart. A wise man listens to cookies.",
    34: "You will die alone and poorly dressed.",
    35: "A fanatic is one who can't change his mind, and won't change the subject.",
    36: "If you look back, you'll soon be going that way.",
    37: "You will live long enough to open many fortune cookies.",
    38: "An alien of some sort will be appearing to you shortly.",
    39: "Do not mistake temptation for opportunity.",
    40: "Flattery will go far tonight.",
    41: "He who laughs at himself never runs out of things to laugh at.",
    42: "He who laughs last is laughing at you.",
    43: "He who throws dirt is losing ground.",
    44: "Some men dream of fortunes, others dream of cookies.",
    45: "The greatest danger could be your stupidity.",
    46: "We don't know the future, but here's a cookie.",
    47: "The world may be your oyster, but it doesn't mean you'll get its pearl.",
    48: "You will be hungry again in one hour.",
    49: "The road to riches is paved with homework.",
    50: "You can always find happiness at work on Friday.",
    51: "Actions speak louder than fortune cookies.",
    52: "Because of your melodic nature, the moonlight never misses an appointment.",
    53: "Don't behave with cold manners.",
    54: "Don't forget you are always on our minds.",
    55: "Fortune not found? Abort, Retry, Ignore.",
    56: "Help! I am being held prisoner in a fortune cookie factory.",
    57: "",
    58: "Never forget a friend. Especially if he owes you.",
    59: "Never wear your best pants when you go to fight for freedom.",
    60: "Only listen to this machine; disregard all other fortune telling units.",
    61: "It is a good day to have a good day.",
    62: "All fortunes are wrong except this one.",
    63: "Someone will invite you to a Karaoke party.",
    64: "",
    65: "There is no mistake so great as that of being always right.",
    66: "You love Chinese food.",
    67: "I am worth a fortune.",
    68: "No snowflake feels responsible in an avalanche.",
    69: "",
    70: "Some fortune cookies contain no fortune.",
    71: "Don't let statistics do a number on you.",
    72: "",
    73: "May you someday be carbon neutral.",
    74: "You have rice in your teeth.",
    75: "Avoid taking unnecessary gambles. Lucky numbers: 12, 15, 23, 70, 37",
    76: "Ask your mom instead of a cookie.",
    77: "Don't eat the paper.",
    78: "Hard work pays off in the future. Laziness pays off now.",
    79: "You think it's a secret, but they know.",
    80: "Change is inevitable, except for vending machines.",
    }
    print ("And your fortune at this Time is :\n\n"+ reading[lucky_number])                  



if __name__ == '__main__':
 
 Thread(target = func1).start()
 time.sleep(2)
 Thread(target = func2).start()









