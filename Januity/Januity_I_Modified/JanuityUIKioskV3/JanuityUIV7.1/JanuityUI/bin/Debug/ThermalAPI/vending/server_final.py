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

machine_id = 2003 #machine ID
print ('Machine ID')
print (machine_id)
os.system("sudo xhost +")
k = PyKeyboard()
print('keyboard initialized')
os.system("sudo chmod a+rw /dev/ttyUSB0") 
ser=serial.Serial('/dev/ttyUSB0',9600)
print(ser.name)
print ('Serial Port Connected')

msg=''

def func5():
    os.system("/var/www/html/frpnew/frpc -c /var/www/html/frpnew/frpc.ini")

def func1():
    print ('Motor Function Started')
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
            i=0
            output=''
            quantity=0
            length=0
            length= len(result)

            print("length",length)
         
            if length == 21:
             quantity = int(result[2:3]) 
             rows = result[0:2]
             payment=result[3:22]
            else:
             quantity = int(result[3:4]) 
             rows = result[0:3]
             payment= result[4:22]   
             #print (result)
            print ("Length :",length)
            print ("Rows :",rows)
            print ("Quantity :",quantity)
            print ("payment id:",payment)
            result = rows;
            while i < quantity:
              #time.sleep(1)
              print (i)

              if result.upper()== 'R1':
               i += 1
               ser.write(str.encode('A'))
               break

              elif result.upper()== 'R2':
               i += 1
               ser.write(str.encode('B'))
               break

              elif result.upper()== 'R3':
               i += 1
               ser.write(str.encode('C'))
               break

              elif result.upper()== 'R4':
               i += 1
               ser.write(str.encode('D'))
               break

              elif result.upper()== 'R5':
               i += 1
               ser.write(str.encode('E'))
               break

              elif result.upper()== 'R6':
               i += 1
               ser.write(str.encode('G'))
               break

              elif result.upper()== 'R7':
               i += 1
               ser.write(str.encode('H'))
               break

              elif result.upper()== 'R8':
               i += 1
               ser.write(str.encode('I'))
               break

              elif result.upper()== 'R9':
               i += 1
               ser.write(str.encode('J'))
               break

              elif result.upper()== 'R10':
               i += 1
               ser.write(str.encode('K'))
               break

              elif result.upper()== 'R11':
               i += 1
               ser.write(str.encode('L'))
               break

              elif result.upper()== 'R12':
               i += 1
               ser.write(str.encode('M'))
               break
            return "Dispense Completed"
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
    global payment
    print ('Serial Port reading started:')
    #os.system("/var/www/html/frpnew/frpc -c /var/www/html/frpnew/frpc.ini")
    while 1:
       #time.sleep(1)
       msg = ser.read(ser.inWaiting())
       msg = msg.decode('utf-8')
       print(msg)
       break
    while 1:
        #msg = ser.read(ser.inWaiting())
        #msg = msg.decode('utf-8')
        while 1:
          #time.sleep(1)
          #time.sleep(10)
          msg = ser.read().decode('utf-8')
          if msg != '':
           print(msg)
           break
        if msg == '0':
         if(is_connected()==True):
          time.sleep(3)
          #print ('Internet Connected')
          mydb = mysql.connector.connect(
          host="18.219.221.179",
          user="root",
          passwd="Mdimsru",
          database="rahasiya_vending_machine")
          mycursor = mydb.cursor()
          sql = f"Update senser_status set status = 0 where machine_id = {machine_id} ".format(machine_id)
          mycursor.execute(sql)
          mydb.commit()
          print("Person UnDetected status to 0\n")
         else:
          print ('No Internet') 
        elif msg == '1':
         if(is_connected()==True):
          #print ('Internet Connected')
          mydb = mysql.connector.connect(
          host="18.219.221.179",
          user="root",
          passwd="Mdimsru",
          database="rahasiya_vending_machine")
          mycursor = mydb.cursor()
          sql = f"Update senser_status set status = 1 where machine_id = {machine_id} ".format(machine_id)
          mycursor.execute(sql)
          mydb.commit()
          print("Person Detected status to 1\n")
         else:
          print ('No Internet')
        elif msg == 'N':
         k.press_key(k.down_key)
         k.release_key(k.down_key)
         print("Up key pressed")
        elif msg == 'O':
         k.press_key(k.left_key)
         k.release_key(k.left_key)
         print("Down key pressed")
        elif msg == 'Q':
         k.press_key(k.up_key)
         k.release_key(k.up_key)
         print("Left key pressed")
        elif msg == 'P':
         k.press_key(k.right_key)
         k.release_key(k.right_key)
         print("Right key pressed")
        elif msg == 'R':
         k.press_key(k.enter_key)
         k.release_key(k.enter_key)
         print("Enter key pressed")
        elif msg == 'S':
         k.press_key(k.backspace_key)
         k.release_key(k.backspace_key)
         print("Backspace key pressed")


        elif msg == 'A':
          print("R1 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R1'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R1'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R1'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()


        elif msg == 'B':
          print("R2 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R2'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R2'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R2'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()



        elif msg == 'C':
          print("R3 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R3'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R3'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R3'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()

        elif msg == 'D':
          print("R4 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R4'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R4'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R4'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()


        elif msg == 'E':
          print("R5 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R5'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R5'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R5'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()


        elif msg == 'G':
          print("R6 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R6'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R6'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R6'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()


        elif msg == 'H':
          print("R7 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R7'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R7'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R7'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()


        elif msg == 'I':
          print("R8 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R8'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R8'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R8'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()


        elif msg == 'J':
          print("R9 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R9'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R9'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R9'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()


        elif msg == 'K':
          print("R10 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R10'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R10'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R10'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()


        elif msg == 'L':
          print("R11 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R11'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R11'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R11'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()


        elif msg == 'M':
          print("R12 Motor Success")
          if(is_connected()==True):
           mydb = mysql.connector.connect(
           host="18.219.221.179",
           user="root",
           passwd="Mdimsru",
           database="rahasiya_vending_machine")
           mycursor = mydb.cursor()
           check_sql = f"Select quantity from products where machine_id = {machine_id} AND rows= 'R12'".format(machine_id)
           mycursor.execute(check_sql)
           check_result = mycursor.fetchall()
           for row in check_result:
            print("Current stock:",row[0])
            mydb.commit()
           if(row[0] > 0):
            sql = f"UPDATE products set quantity= quantity -1 where {machine_id} AND rows= 'R12'".format(machine_id)
            mycursor.execute(sql)
            mydb.commit()
            print("Quantity Updated\n")
           else:
            print("No stock Available\n")
            sql_update = f"UPDATE products set quantity= 0 where machine_id = {machine_id} AND rows= 'R12'".format(machine_id)
            mycursor.execute(sql_update)
            mydb.commit()

       
        elif msg == 'F':
          try:
           print("Motor Failure")
           print(payment)
           # open a connection to a URL using urllib
           webUrl  = urllib.request.urlopen(f'http://srushty.com/raha_refund_failed.php?payment_id={payment}'.format(payment))
           #get the result code and print it
           print ("result code: " + str(webUrl.getcode()))
           # read the data from the URL and print it
           data = webUrl.read()
           print (data)
          except OSError as e:
           print(e.errno)
           pass
          finally:
           print(payment)
           print("Refund Success")
           

REMOTE_SERVER = "18.219.221.179"
def is_connected():
  try:
    host = socket.gethostbyname(REMOTE_SERVER)
    s = socket.create_connection((host, 80), 2)
    print ('Server Connected')
    return True
  except:
    print ('server Disconnected')
    pass
  return False

if __name__ == '__main__':
 Thread(target = func5).start()
 Thread(target = is_connected).start()
 Thread(target = func1).start()
 time.sleep(2)
 Thread(target = func2).start()
 









