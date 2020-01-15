Configuration
==============================================

Configuring build is primarily concerned with listeners and trace level as they directly affect how the output from Bilge is sent to the chosen listener.  Bilge uses
listeners to store the output of all of the trace and they must be added to the application for any trace to be written. 

Trace also needs to be turned on, the default for Bilge is that all trace is disabled, this is a safe default ensuring that the performance is maximised and that
you are not accidentally streaming developer trace during your production deployments.  However the expected use case is that developers turn on trace for their
development and testing activities.

Adding listeners


There is only a single sample listener in the Plisky.Diagnostics nuget package.  This is so that the package can be used standalone but it is not expected that this
listener is used in any real use case.  For that the Plisky.Diagnostics.Listeners Nuget Package is required.

Listeners Included in Plisky.Diagnostics.Listeners

    * Plisky.Diagnostics.Listeners.TCPHandler
    * Plisky.Diagnostics.Listeners.ODSHandler
    * Plisky.Diagnostics.Listeners.InMemoryHandler
    * Plisky.Diagnostics.Listeners.FileSystemHandler



Plisky.Diagnostics.Listeners.TCPHandler


This is the default listener and streams messagees over TCP to FlimFlam, a custom application designed to capture these messages from multiple processes and display
the contents.  


Plisky.Diagnostics.Listeners.ODSHandler


This listener uses the system "OutputDebugString" win32 API call to send the trace messages.  It is designed for interoperability with other viewers that capture
messages from this API call.  FlimFlam is able to capture these messages too.  

Plisky.Diagnostics.Listeners.InMemoryHandler


This is a queued handler that does not send the output anywhere at all but keeps a configurable number of messages within the memory space of the application. This
is designed to be used for light weight or  production debugging scenarios where only the most recent XXXX number of messages are to be captured.  To use this 
effectively other code needs to be written to take the messages and do something with them.

Plisky.Diagnostics.Listeners.FileSystemHandler


This is basic file system logging, designed to create logfiles on disk for Bilge trace.


Custom Handlers


It is expected that developers will have requirements that are not met by any of these handlers, and custom handlers can be made and added through the same mechanism.