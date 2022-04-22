#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import serial
import crcmod.predefined
import threading
import multiprocessing as mp
import time
import math
from threading import Thread


class EvoSerialTriggerProcess(Thread):
    TEXT_MODE = b"\x00\x11\x01\x45"
    BINARY_MODE = b"\x00\x11\x02\x4C"
    SINGLE_PIXEL_MODE = b"\x00\x21\x01\xBC"
    TWO_BY_TWO_PIXEL_MODE = b"\x00\x21\x02\xB5"
    TWO_PIXEL_MODE = b"\x00\x21\x03\xB2"
    SHORT_RANGE_MODE = b"\x00\x61\x01\xE7"
    LONG_RANGE_MODE = b"\x00\x61\x03\xE9"

    def __init__(self, portname, threshold, shared_variables, debug=False):
        #mp.Process.__init__(self)
        Thread.__init__(self)
        print("Evo Mini Process Status ")
        self.portname = portname
        self.debug = debug
        self.trigger_threshold = threshold
        self.trigger_event = shared_variables["recording_trigger"]
        self.distance_queue = shared_variables["distance_queue"]
        self.last_time_triggered = time.time()

        self.port = serial.Serial(
            port=portname,
            baudrate=115200,
            parity=serial.PARITY_NONE,
            stopbits=serial.STOPBITS_ONE,
            bytesize=serial.EIGHTBITS
        )
        #print(f"Evo - MINI: {self.port}")
        self.crc8 = crcmod.predefined.mkPredefinedCrcFun('crc-8')
        self.serial_lock = threading.Lock()

        self.port.flushInput()
        self.set_binary_mode()  # Set binary output as it is required for this sample

        # Set ranging mode
        self.set_long_range_mode()
        # self.set_short_range_mode()

        # Set pixel mode
        self.set_single_pixel_mode()
        self.run()

    def print_debug(self, thing_to_print):
        if self.debug:
            print(thing_to_print)

    def detect_motion(self, current, last):
        detected = False
        if not (math.isinf(current) or math.isinf(last)):
            if 45 < current <= self.trigger_threshold:
            	#print("detected above 45",self.trigger_threshold)
            	detected = True
                #print("detect_motion...")
            else:
                detected = False
                #print("detected motion below 45")
        return detected

    def run(self):
        if self.debug:
            print("[Evo Serial process] Starting...")
        try:
            last_range = float('inf')
            while True:
                current_range = self.get_ranges()
                self.distance_queue.put(current_range[0])
                #print("evo mini run",self.distance_queue)
                #print("evo mini run",current_range)
                motion_detected = False
                if current_range is not None:
                    motion_detected = self.detect_motion(
                        current_range[0], last_range)
                    #print("evo mini run",current_range[0])
                    last_range = current_range[0]
                    #print(last_range)
                if motion_detected and not self.trigger_event.is_set():
                    if self.debug:
                        print("[Evo Serial process] {}: Motion detected at {} cm".format(
                            str(int(time.time())), current_range[0]))
                    self.last_time_triggered = time.time()
                    self.trigger_event.set()
                    #print("kishore",self.trigger_event)
                    self.port.flushInput()

        except Exception:
            if self.debug:
                print("Serial process exception")
            raise

    def get_ranges(self):
        # Read one byte
        ranges = []
        data = self.port.read(1)
        if data == b'T':
            # After T read 3 bytes
            frame = data + self.port.read(3)  # Try a single-range frame
            if frame[-1] != self.crc8(frame[:-1]):
                frame = frame + self.port.read(2)  # Try a two-range frame
                if frame[-1] != self.crc8(frame[:-1]):
                    # Try a two-by-two-range frame
                    frame = frame + self.port.read(4)
                elif frame[-1] != self.crc8(frame[:-1]):
                    return "CRC mismatch. Check connection or make sure only one progam accesses the sensor port."

            # Convert binary frame to decimal in shifting by 8 the frame
            for i in range(int((len(frame) - 2) / 2)):
                rng = frame[2 * i + 1] << 8
                rng = rng | (frame[2 * i + 2] & 0xFF)
                ranges.append(rng)
        else:
            return "Wating for frame header"
        # Check special cases (limit values)
        self.check_ranges(ranges)

        return ranges

    def check_ranges(self, range_list):
        for i in range(len(range_list)):
            # Checking error codes
            if range_list[i] == 65535:  # Sensor measuring above its maximum limit
                range_list[i] = float('inf')
            elif range_list[i] == 1:  # Sensor not able to measure
                range_list[i] = float('nan')
            elif range_list[i] == 0:  # Sensor detecting object below minimum range
                range_list[i] = -float('inf')
            else:
                # Convert frame in centimeters
                range_list[i] /= 10.0

        return range_list

    def send_command(self, command):
        with self.serial_lock:  # This avoid concurrent writes/reads of serial
            self.port.write(command)
            ack = self.port.read(1)
            # This loop discards buffered frames until an ACK header is reached
            while ack != b"\x12":
                ack = self.port.read(1)
            else:
                ack += self.port.read(3)

            # Check ACK crc8
            crc8 = self.crc8(ack[:3])
            if crc8 == ack[3]:
                # Check if ACK or NACK
                if ack[2] == 0:
                    return True
                else:
                    if self.debug:
                        print("Command not acknowledged")
                    return False
            else:
                if self.debug:
                    print("Error in ACK checksum")
                return False

    def set_binary_mode(self):
        if self.send_command(EvoSerialTriggerProcess.BINARY_MODE):
            if self.debug:
                print("Sensor succesfully switched to binary mode")

    def set_two_by_two_pixel_mode(self):
        if self.send_command(EvoSerialTriggerProcess.TWO_BY_TWO_PIXEL_MODE):
            if self.debug:
                print("Sensor succesfully switched to 2 by 2 ranges measurement")

    def set_two_pixel_mode(self):
        if self.send_command(EvoSerialTriggerProcess.TWO_PIXEL_MODE):
            if self.debug:
                print("Sensor succesfully switched to two ranges measurement")

    def set_single_pixel_mode(self):
        if self.send_command(EvoSerialTriggerProcess.SINGLE_PIXEL_MODE):
            if self.debug:
                print("Sensor succesfully switched to single range measurement")

    def set_short_range_mode(self):
        if self.send_command(EvoSerialTriggerProcess.SHORT_RANGE_MODE):
            if self.debug:
                print("Sensor succesfully switched to short range measurement")

    def set_long_range_mode(self):
        if self.send_command(EvoSerialTriggerProcess.LONG_RANGE_MODE):
            if self.debug:
                print("Sensor succesfully switched to long range measurement")
