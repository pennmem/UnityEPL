#############
FAQ
#############
This is the coding practices document for the UnityEPL

.. contents:: **Table of Contents**
    :depth: 2

*************
General
*************
These are general questions often asked 

=============
What does the name stand for?
=============
UnityEPL stands for Unity Experiment Programming Library


*************
Common Unity Errors
*************
These are general questions often asked 

=============
You just imported UnityEPL and there are a bunch of errors
=============
Make sure you close the unity editor and re-open it.
I'm not sure why this is needed, but it is.

=============
InterfaceManager accessed before Awake was called
=============
#. Click *Edit > Project Settings*
#. Go to *Script Execution Order*
#. Click the *+* to add a script and select UnityEPL.InterfaceManager
#. Set the value of this new item to *-10* (or anything less than 0)

=============
You start the experiment and it hangs
=============
Check that you don't have two experiments active in your scene

