# -*- coding: utf-8 -*-
"""
Created on Sun May 10 19:20:16 2020
Terabee SAS (c) 2020

State machine implementation
"""
from API_events import APIevent


class StateMachineEvent(object):
    READY = 0
    ALIGNMENT = 1
    MEASURING_START = 2
    MEASURING_END = 3
    LOW_POWER_TIMEOUT = 4
    MEASURING_FAIL = 5
    NONE = -1


class State(object):
    """Generic Sate object definition.

    We define a state object which provides some utility functions for the
    individual states within the state machine.
    """

    def __init__(self, parent_state_machine):
        """Constructs state object"""
        self.parent_state_machine = parent_state_machine

    def next(self, input):
        """Check for transitions from given inputs."""
        if not isinstance(self, type(self.parent_state_machine.previous_state)):
            self.parent_state_machine.previous_state = self

    def __repr__(self):
        """Leverages the __str__ method to describe the State."""
        return self.__str__()

    def __str__(self):
        """Returns the name of the State."""
        return self.__class__.__name__


class StateMachine(object):
    """A simple state machine class."""

    def __init__(self, initial_state, debug=False):
        """Initialize the components."""
        # Start with a default state.
        self.current_state = initial_state
        self.previous_state = None
        self.debug = debug

    def display_transition(self, next):
        """Display transition."""
        if next.__class__.__name__ != self.current_state.__class__.__name__:
            transition_str = "Transition from "
            transition_str += self.current_state.__class__.__name__
            transition_str += " to "
            transition_str += next.__class__.__name__
        else:
            transition_str = "Stay in "
            transition_str += next.__class__.__name__

        print(transition_str)

    def update(self, input):
        """Forward input to current state to checck for transition."""
        next_state = self.current_state.next(input)
        if self.debug:
            print(self.previous_state)
            self.display_transition(next_state)
        self.current_state = next_state


class Initial(State):
    def next(self, input):
        """Check for transitions from given inputs."""
        State.next(self, input)
        if input == StateMachineEvent.READY:
            next_state = Ready(self.parent_state_machine)
            self.parent_state_machine.notify_event(APIevent.SYSTEM_READY)
        else:
            next_state = self

        return next_state


class Ready(State):
    def next(self, input):
        State.next(self, input)
        if input == StateMachineEvent.ALIGNMENT:
            next_state = Alignment(self.parent_state_machine)
            self.parent_state_machine.notify_event(APIevent.PERSON_DETECTED)
        elif input == StateMachineEvent.MEASURING_START:
            next_state = Measuring(self.parent_state_machine)
            self.parent_state_machine.notify_event(APIevent.MEASUREMENT_START)
        else:
            next_state = self
        return next_state


class Alignment(State):
    def next(self, input):
        State.next(self, input)
        if input == StateMachineEvent.MEASURING_START:
            next_state = Measuring(self.parent_state_machine)
            self.parent_state_machine.notify_event(APIevent.MEASUREMENT_START)
        elif input == StateMachineEvent.READY:
            next_state = Ready(self.parent_state_machine)
            self.parent_state_machine.notify_event(APIevent.SYSTEM_READY)
        else:
            next_state = self
        return next_state


class Measuring(State):
    def next(self, input):
        State.next(self, input)
        if input == StateMachineEvent.MEASURING_END:
            next_state = DisplayResult(self.parent_state_machine)
            self.parent_state_machine.notify_event(APIevent.RESULT_READY)
        elif input == StateMachineEvent.MEASURING_FAIL:
            next_state = DetectionFail(self.parent_state_machine)
            self.parent_state_machine.notify_event(APIevent.CANNOT_MEASURE)
        elif input == StateMachineEvent.READY:
            next_state = Ready(self.parent_state_machine)
            self.parent_state_machine.notify_event(APIevent.SYSTEM_READY)
        else:
            next_state = self
        return next_state


class DetectionFail(State):
    def next(self, input):
        State.next(self, input)
        if input == StateMachineEvent.READY:
            next_state = Ready(self.parent_state_machine)
            self.parent_state_machine.notify_event(APIevent.SYSTEM_READY)
        else:
            next_state = self
        return next_state


class DisplayResult(State):
    def next(self, input):
        State.next(self, input)
        if input == StateMachineEvent.READY:
            next_state = Ready(self.parent_state_machine)
            self.parent_state_machine.notify_event(APIevent.SYSTEM_READY)
        else:
            next_state = self
        return next_state


class FeverScreeningStateMachine(StateMachine):
    def __init__(self, event_callback=None, debug=False):
        """Init state machine with initial state."""
        StateMachine.__init__(self, Initial(self), debug)
        self.event_callback = event_callback

    def notify_event(self, event):
        """Function called by states on event."""
        if self.debug:
            print("Event received", event)

        if self.event_callback is not None:
            self.event_callback(event)

    def state_machine_current_state(self):
        return str(self.current_state.__class__.__name__)
