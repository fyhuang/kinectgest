CS228 Final Project - segmentation-based gesture recognition
Yifeng Huang <fyhuang> and Ivan Zhang <zhifanz>


1. Compiling

The code has a few dependencies:
    * OpenTK <http://www.opentk.com/>, a graphics library
    * ZedGraph <http://zedgraph.org/> (use Web Archive, the real site is down), for plotting
Put these dependencies in a directory called "Deps", so that the Deps directory looks like this:

Deps/
Deps/ZedGraph.dll
Deps/opentk/
Deps/opentk/Binaries/
Deps/opentk/Documentation/
etc.

You can use Visual Studio 2010 or MonoDevelop to compile the code. The code should run smoothly on Windows, Mac OS X, and Linux (the latter two with Mono <www.mono-project.com>, although plotting doesn't work on Mac). The binaries should also be portable.


2. Running

There are two projects included: FinalProject, and LogFileVisualizer. Both should be run from a command-line (they take arguments) in the root directory: i.e. the one that contains Deps/, FinalProject/, LogFileVisualizer/, etc. Before running the programs, make sure the following things are present in the root directory:

gestures/track_{gesturename}_??.log
gestures/frames/ns_??.log
models/ (empty)

The gestures/ directory contains all the training and test data. (We use cross-validation.) The frames/ subdirectory contains log files which train the neutral stance feature. For sample training data, see:

http://huang.stanford.edu/gestures3.tar.bz2

(This link will not be present indefinitely.)


FinalProject.exe has help text if you run it with no arguments. Explanations of possible actions:
    * Train - trains the models and saves them to the models/ directory
    * TestRecognize - tests the model on the test set
    * CycleCV - use the next cross-validation set
    * TestRealtime - pass one of the combined tracks as the second argument
    * TestSingle - tests recognizing a single gesture from a file
    * BenchmarkRecognize - (not sure that this works) see how many gestures we can recognize per second

LogFileVisualizer.exe has no help text. If you run it with no arguments, it opens a high_kick. The arguments are reversed: the filename is first, and the action (optional) is second. Default action is ViewGesture.
    * ViewGesture - 3D visualization of the gesture file (see below)
    * PlotJoint - show X,Y,Z and angle plots of a joint from a gesture file (it will ask you for the joint to plot; joint names include "right-wrist", "left-ankle", etc.)
    * PlotJointsFromGestures - instead of passing in a filename, pass as the first parameter a gesture name (i.e. "high_kick"). This will plot (using many windows) all the high-kicks.

ViewGesture controls:
    * Escape        quit
    * A,Z           zoom in/out
    * Left/Right    rotate l/r
    * Up/Down       time advance/reverse
    * R             rewind (reset to beginning)
    * S             output on stdout the parameters of the current state (useful for recording training data for neutral stance)
    * Left Shift    hold to slow down speed of camera rotate and frame advance

Note that frames are interpolated, so even slow movement is quite smooth. In addition, the 'S' key outputs interpolated frames.
