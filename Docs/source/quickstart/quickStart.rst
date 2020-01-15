
==================
Quick Start Guide.
==================

Bilge is a class designed to add trace to your code.  

1 - Add a nuget reference to Plisky.Diagnostics and Plisky.Diagnostics.Listeners.

2 - Add the code to initialise Bilge

.. code-block:: shell
    
    Bilge b = new Bilge(tl:TraceLevel.Verbose);
    b.AddListener(new TCPListener("127.0.0.1",9060));
    b.Info.Log("Hello World!");
    // Do some real code or just...
    Thread.Sleep(1);
    // This is not needed, but for this example we'll include it
    b.Flush();


3 - Open Up FlimFlam

Flim Flam is a windows application deisgned to intercept your trace messages at run time. 

4 - Run your Program!


Bilge works by adding a listener, the simplest listener to use is often the TCP one which will stream locally to the same machine using the loopback address
or across the network if that is desired.  

    Be careful when streaming developer trace across a network, it is not encrypted and is designed to aid diagnostics.  

Bilge is inherantly multithreaded and therefore if you run this code in a console application without the sleep command its entirely possible that the program 
exits and the trace is lost prior to it being written.   This is deliberate as Bilge uses background threads internally to ensure that large quantities of trace
do not keep the applicaiton running after it should have terminated.  To avoid this you can use b.Flush();

