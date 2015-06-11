VVVV Emotiv Epoc
================

Node to use the Emotiv Epoc with VVVV. It allows you to access the different components of EmoStates such as:
* Expressiv: Facial expressions
* Affectiv: Internal states
* Cognitiv: Mental tasks

For license purposes, you will need to get the following dlls on your own (from your EmotivSDK folder), either to use or build the plugin:
* `edk.dll`
* `DotNetEmotivSDK-vs2010.dll`

If you want to build the plugin yourself, put both dlls in the appropriates `src/dependencies/{x86,x64}` folders. You need to copy them to the bin folder as well.