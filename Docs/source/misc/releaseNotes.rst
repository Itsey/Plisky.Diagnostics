
Release Notes
==============================================

Release notes for the most recent minor version transition are a part of the nuget, all others remain here with further details.
2.8.1 - Handlers Bug Fix.
      SimpleFileHandler bug prevented writing, caused null reference exception on logging. This is now fixed.
      FileSystemHandler REMOVED, incomplete and buggy code shouldnt have been present.  Not flagged this as an interface break as the 
      handler was so buggy it could not have been used.
    
2.8.0 - 
      INTERFACE BREAK.... TraceLevel is now SourceLevels, new Feature Bilge.SetConfigurationResolver allows SourceSwitches and TraceSwitches.
      Move to adopt source levels from more recent frameworks.  Still incompatble with Core but Source Levels works with framework and core 
      so will settle on that.
      New Feature - Bilge.SetConfigurationResolver is a function called each time that a new intance of Bilge is created, passing in the 
      instance name of that instance.  This can be determined to turn on or off tracing for subsystems.  Each instance call hits your call back
      which can then read the desired trace levels from app configuration or elsewhere. Net framework default implementation supports traceswitches.  
     
2.7.2 - Missing method exception issues resolved  (Critical fix for 2.7.X and 2.6.X)
      Critical fix - series of issues around the change of the constructor signatures all versions from 2.6.0 through 2.7.2 are now invalid.

2.7.0 - Utter Mare on constructors, changing interface.
2.6.2 - Bug Fix on Constructor causing issues with overloads ( added in new constructors - Critical fix for 2.6.2)
2.6.2 - Adding Process Name support in.
2.6.0 - Remove signing accross Plisky
2.4.0 - Version harmonisation, correcting fault in net standard version.
2.3.3 - Fix for containers not being able to get machine name. (Critical fix for 2.3.2).
2.3.1 - Bug Fix - Net40 support was not correctly implemented.
2.3.0 - New Feature - MessageBatching is Implemented.  XmlDocs included in package.
2.2.3 - Moving Legacy Formatter to Bilge not Handlers to ease handler development.
2.2.2 - Adding new GitHub Hosting Url.