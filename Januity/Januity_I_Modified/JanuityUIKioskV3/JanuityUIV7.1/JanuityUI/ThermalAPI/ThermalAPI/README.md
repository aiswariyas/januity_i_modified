# README #

### Requirements ###
# Installation of python3 requirements
If both python 2 and python 3 are installed on your machine, use the following:

>pip3 install --user  -r "requirements.txt"

If you have only python3 installed on your machine, use the following:

>pip install --user  -r "requirements.txt"


### Usage ###

* Initialization

Start by importing the fever detection object and the APIevent enum to your software. The fever detection API runs as a separate thread, you will need to feed it an event for stopping it. It also takes the ports, temperature unit (C or F) and optionally, a callback function for when an event is registered as keyword parameters. The APIevent is used by the callback to notify you of events. Example usage:
```
from fever_detection import FeverDetection
from multiprocessing import Event

MyDetector = FeverDetection(portnames={'evo_thermal_port': "/dev/ttyACM0",  # modify this accordingly
                                   '	evo_mini_port': "/dev/ttyACM1"},  # modify this accordingly
                        		settings={"unit": "C"}, stopEvent=stopEvent,
                        		callback=callback)
```

* Initializing the sensors

After it was declared, the function `initialize()` needs to be called. This will start the sensor processes and initialize the internal state machine. Example:
```
MyDetector.initialize()
```

* Starting the system

Since this is a Thread object, you will need to call its `start()` method in order to run it. After this, it now run in an infitine loop until the stop event is received.
```
MyDetector.start()
```

* Receiving the data

There two ways to get data out of the system. You can either poll the `getStatus()` method or initialize it with a callback function and work with that. Of course, you can also combine the two methods. Both the callback and the `getStatus()` method will return a string that looks like this: *"Current state: Alignment, Current distance: 56.0, Current temperature: 0.0"*. However, there is one difference: the callback will feed you an APIevent such as *PERSON_DETECTED* or *SYSTEM_READY* since its purpose is to provide feedback on events. Whereas the `getStatus()` method will return the current state of the state machine -> you can easily match the event to the state since they are similar in meaning and syntax. The `event` argument is an `APIevent` while the `temperature` is of type `float`. Example usage:
```
# Polling method
MyDetector.getStatus()

# Callback method
def callback(string, event, temperature):
    print("In main thread callback function. Input: ", string)
    if event == APIevent.RESULT_READY:
        print("Current temperature", temperature)
```

* Detection failure

In case the thermal cannot get a temperature reading, the API will notify with the callback and then sleep for 1 second and then reset the state machine.

* Stopping the system

In order to stop the API, you will have to set the stopEvent to True and then call the `join()` method of the thread. Example:

```
stopEvent.set()
MyDetector.join()
```

# Quick start script
All the example code above is located in the test_script.py file and can be run immediately after all the dependencies are installed. You will need to modify the ports to the ones that correspond to the Evo Mini and Evo Thermal 33 on your setup (see Initialization). After modifying the ports, run the script by typing in:

>python3 test_script.py


