# -*- coding: utf-8 -*-
"""
Created on Fri Jun 12 07:33:03 2020
Terabee SAS (c) 2020

"""
from enum import Enum


class APIevent(Enum):
    SYSTEM_NOT_READY = -1  # system is not fully initialized / started
    SYSTEM_READY = 0  # system is ready for measurement
    PERSON_DETECTED = 1  # person within 1 meters
    # person at correct distance ( 45 <= distance <= 60*) trigger at 55
    MEASUREMENT_START = 2
    RESULT_READY = 3  # result of measurement ready
    CANNOT_MEASURE = 4
    """
    Thermal could not get a measurement.
    Possible reasons: person is wearing glasses and mask, or some item covering
    most of their face. Or measurement started normally but person moved out
    of bounds
    """
