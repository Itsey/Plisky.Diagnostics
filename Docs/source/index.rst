.. Plisky.Diagnostics documentation master file, created by
   sphinx-quickstart on Wed Dec 18 19:50:59 2019.
   You can adapt this file completely to your liking, but it should at least
   contain the root `toctree` directive.

================================
Plisky.Diagnostics Documentation
================================

Plisky.Diagnostics is a Nuget Package which provides Bilge as a developer oriented trace framework for use with .net and .net core applications.  Bilge is designed
to allow you to write trace statements into your .net code to aid readability and help debug and diagnose issues with code.

.. code-block:: shell
    
    Bilge b = new Bilge(tl:TraceLevel.Verbose);
    if (user.IsValid) {
        b.Info.Log("Valid user selected");
    }
    

To get up and running as fast as possible follow this guide.....  See :doc:`/quickstart/quickStart`

.. toctree::
   :maxdepth: 2
   :caption: Contents:

   contents.rst
   overview/introduction   
   overview/configuration
   quickstart/quickStart
   misc/noTrace
   

Contents
==================

* :ref:`genindex`
* :ref:`modindex`
* :ref:`search`
