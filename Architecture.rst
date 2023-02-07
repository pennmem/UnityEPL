#############
Coding Practices
#############
This is the coding practices document for the UnityEPL

.. contents:: **Table of Contents**
    :depth: 2

*************
Overview
*************
There are certain coding practices that should always be applies when coding in the UnityEPL.
Some are recommendations and some are requirements (code will break if you do not follow it).
We try to make as many errors occur at compile time, but there are always limitations of the language.

*************
Important Code Structures
*************
These are the important structures within the code.

=============
InterfaceManager
=============
The InterfaceManager has two main jobs
#. Hold the objects for all EventLoops
#. Allow other event loops to interact with unity objects/functions

This is a bit monolithic, but is allows the objects to be held in one consistent object and allows for a clean interface with Unity.
This also means the InterfaceManager is running on the main unity thread. 

=============
Events/Tasks
=============
Events are async functions that run on an EventLoop thread.

There are 3 types of events:
#. *Do*
#. *DoWaitFor*
#. *DoGet*

A *Do* event creates an async function running on the receiving class' EventLoop and immediately continues in the current context. 
A *DoWaitFor* event creates an async function running on the receiving class' EventLoop and then you can perform an async await for it to be done in the current context.
A *DoGet* event is just like DoWaitFor, but it also returns a value

These tasks only allow up to 4 blittable types to be passed into them. Blittable types are stack based types that contiguous in memory. More importantly, they can't contain references.
This is important because if you access the value of a reference across thread, it will cause race conditions. 
That is, unless it is a thread safe concurrent datatype. If you know this is the case, and you are REALLY sure you know what you're doing, and there is no other way to architect it, then you can use a lambda to grab that value as a reference and pass the lambda into your Event.

NOTES:
* If you need to pass more than 4 arguments, make a struct and pass that in.
* Prefer using *Do* over *DoWaitFor* and *DoGet*

*************
Important Coding Practices
*************
These are the important practices that are critical for all coders to understand and follow.
- Do NOT use *Task.Delay()*. Instead, use *InterfaceManager.Delay()*. They act exactly the same, but InterfaceManager.Delay knows how to handle the single threaded nature of webgl

*************
Acronyms and Terms
*************
Below are the common acronyms and terms used in this project

=============
Acronyms
=============
* EEG = Electroencephalogram

=============
Terms
=============
* Elemem = CML EEG reading and stimulation control system
