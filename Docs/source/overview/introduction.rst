
Introduction to Bilge and FlimFlam
==============================================

Plisky.Diagnostics is a nuget package hosting Bilge - an advanced trace library designed for adding developer trace to .net applications.  Bilge uses a system of
logging and logger implementations to stream out trace data to different sources. However it is most powerful when used in conjunction with FlimFlam, a viewer
designed to read the Bilge trace stream and make it simpler to work with.

Bilge is a general purpose logger though and does not have to be used with FlimFlam but can be used with any standard listener from the .net framework or by 
creating a custom listener yourself or using one of the ones shipped in Plisky.Diagnostics.Listeners.  