Source: See Github "Download Zip" link on the right.
Program(already compiled EXE): https://github.com/AaronLS/CellularAutomataAsNeuralNetwork/releases

CellularAutomataAsNeuralNetwork
===============================
This was the project I wrote while learning C# for the first time, many years ago, and I also wrote an undergraduate thesis on the mathematics of converting outer totalistic cellular automata into a neural network.  It is pretty common to convert one computational system into another, for various reasons(perhaps the tools you have at hand are better at processing the second computational system), therefore it is potentially useful to know how to convert one system to another.  Whether this particular conversion has practical applications is unknown to me.  One possible application would be if you had a system that you were able to model satisfactorily with a cellular automata, but wanted to use a specialized neural network chip(a chip specialized for processing neural networks which would perform many times faster than a conventional chip at the same task) or perhaps in the future a similar memristor chip.  This is pure speculation on my part, as the firing function my neural network uses is not a simple threshold.  It has several thresholds firing zones.  For Conway's  Game of Life, the firing threshold is between .28 and .4, so the input neuron connections must be within that range, as opposed to a more traditional firing theshold of just bein grater than an amount.  More complex cellular automata that you design with my program might have several zones.

Runs John Conway's Game of Life or any other outer totalistic cellular automata you define, but internally cells states are represented as neuron firing states, and rules are converted to neuron waits and firing function thresholds.

Disclaimer
==========
This application was originally written against .NET 1.1, and therefore by today's standards might be considered poorly written, as there is a good bit of non-generic collections which aren't typesafe(I actually had an internal conflict about this, as I was remotely familiar with Java which at the time DID have generics, and I was frustrated by the lack in C#).  It was also my very first C# application, and I am posting only because it 1) Is pretty fun to play with and make your own cellular automata 2) I spent alot of time optimizing it so it would run with exceptable performance 3) Many people who have seen it in action have asked for the source code.

Additionally, modelling neural networks varies by a great degree in terms of both artificial vs natural modelling, as well as simplistic to complex/accurate modelling.  Natural neurons usually do not have quite so distinct states as firing and not firing,  My usage of the term neural network refers as at the simplistic artificial end of the spectrum.

If you flame me for coding style or architecture, I will point you to this disclaimer and you must admit you are an idiot for basically flaming someone for something that any sane person would expect to have mistakes(given that it was my first C# application and also written before many of today's C# language constructs existed).

Known Bug
=========
The process often remains open when the window is closed.  I never bothered to fix this as I wasn't planning to release to the public, and it is easy enough to Ctrl+Shift+Esc and close the process.

Architecture
============
Platform: Windows 32bit/x86
UI: Windows Forms
IDE: Visual Studio, current *sln file opens in VS 2012 Express for Desktop or VS 2012 Standard or greater.  I created the project originally under VS 2003 with .NET 1.1, so there is no reason you shouldn't be able to make a older sln and csproj files for an older VS.
Language: C#
Framework: Code is compatible with Microsoft .NET 1.1, in the current solution I have tested against both 2.0 and 4.5

The main challenge was getting all those squares to update each time step in a smoothly and quickly.  This was initially painfully slow on computer hardware 10 years ago.  This simple architecture resolved this issue.

A background thread(Modelling.cs) models the neural network and adds updates to a queue, while the UI(UIForm.cs) thread consumes that queue and displays the updates via double buffered GDI.  Even though I had a single core processor at the time you might think multithreading would convey no benefits, but updating a UI thread from a background thread usually requires marshaling the calls, which degrades performance.  Additionally, if this application were single threaded, the UI would often freeze while the neural network was updating(this can be mitigated in a *hackish* way by judicial use of DoWork).  However, using a queue to communicate updates from one thread to another avoids making marshaled UI thread calls and allows the UI to remain responsive while the other thread performs neural network modelling.

Both neural networks(see disclaimer on terminology) and simple cellular automata operate in 2 distinct state of alive/firing and dead/not-firing.  Both also consist of many identical units, neurons and cells, which all operate simultaneously.  Since a 64x64 grid of cells consists of over 4000 cells, we cannot have simultaneous execution of all 4000 cells without a 4000 core processor.  In the absence of such a processor, we must simulate this simultaneous execution by maintaining two models of the system: the current time step, and the next time step(or previous and current depending on how you look at it).  The current time step has the states of all 4000 cells, whether they are alive or dead.  As we calculate the state of the cell in the next time step, we must use the states of neighbors in the current time step, but update the state of the cell only in the next time step.  Otherwise, if we didn't have a next and current, imagine if
  -We must go cell by cell to update the states of all cells from timestep 1 to timestep 2.
  -Cell A in time step 1 is alive, but in timestep 2 we calculate that it is dead based on inputs from neighboring cells, and update its state.
  -Cell B in time step 1 is dead, and based on the inputs of its neighbors, we calculate its state in timestep 2.  The states we use from its neighbors should all be their states in timestep 1, however, we already updated Cell A, and therefore are using an invalid future state for the calculation.
Thus you see why it is so important to "buffer" these changes until you have processed all cells, then swap the next time step with the current.

There are some thread synchronization techniques demonstrated in this project.  Most of them I learned from reading articles at the time, so hopefully they are not poor examples.  However, I will say in my many years of programming, while I have used background threads, I have usually designed my applications to avoid any need for this kind of low level thread synchronization *because it is painful, tedious, and error prone to write*.  I generally avoid writing low level thread sync code by instead using constructs of background threads of the .NET framework and events like ProgressChanged/RunWorkercompleted, or the new async/await constructs added to C#.  I'm not saying low level thread sync' is obsolete, but many common scenarios are more easily implemented using other techniques.  Low level thread synchronization is probably still applicable where your needs don't fit one of those scenarios, and you feel you could *significantly improve performance*.  This implies your that there is both a large potential performance gain to be had, *and* your skills make you capable of realizing those gains without also introducing thread safety bugs.  For example, this application has a bug that causes the process to often remain open when the window is closed, which I suspect is related to the background thread.

Binary serialization is demonstrated, showing how to save the state of the cells and rules to a file, and then later load that file to restore those cells/rules.  It is generally a better practice to use some other serialization such as XML or JSON(my personal favorite) which is human readable.  The reason this is favored over binary is because it allows others look at the file and easily see its structure, and thus create programs to read these files and modify them in an automated way, or even manually edit them in a text editor.  Perhaps you wanted to make a HTML5 or java applet that can load rules created in my application, and display the cellular automata in a browser.  If the file is in a human readable format, it makes it much easier for you to determine its structure and thus write code to load it.  Additionally most other languages have libraries that support JSON and XML.  A binary format, while not impossible to figure out, would be much more time consuming and error prone to reverse engineer.  However, binary formats load/save generally faster and take up less space, but some of today's JSON processors are very fast and the performance benefits of binary are becoming negligible.

Usage
=====
Once you have compiled and run the program:
Left click a blue square to turn it red.  Right click to turn it back blue.  Both left/right click support dragging to draw cells.  Note that this is somewhat imperfect, and fast drags will miss cells, since it does not interpolate skipped cells(i.e. when you drag very quickly Windows does not trigger the event on some cells, and you would need to write code to calculate a line between previous cell to know which cells inbetween to update).

**Start**: Begins stepping the neural network/cellular automata at a rate controlled by the slider on the left. Slider: Moving the slider all the way down attempts to process one step per millisecond, but it is unlikely to run quite that fast even on newer hardware(there are 4000 neurons and over 30000 neuron connections to process each timestep).

**Stop**: Pauses processing until you click Start again.

**Step**: Steps forward a number of steps in the box on the right.  Since the UI is not updated during this processing, it allows faster processing to occur over a large number of steps.  Or if you are wanting to study how a cellular automata progresses step by step, allows you to control stepping one step at a time. 

**Clear**: Clears the cells setting them to dead/not-firing, but leaves existing rules in place.

**Save**: Saves the existing cell states and rules to a file.

**Load**: Load a previously saved file.

**New**: Clears current rules and allows you to define a new outer totalistic cellular automata.
  
  -Totalistic Automata basically have rules that are based on a count of the number of neighboring cells that are in a certain state.  My program only supports 2 states: alive and dead("asleep" if you want to keep it G rated).
  
  -After clicking new, you will be in a totally different mode(although the UI does not change much to indicate this), presented with a single white square, and red squares define the *relative* position of neighbors.  This allows you to define the neighborhood, where cells are counted for rules.  For example, the Game of Life only counts the immediate neighbors of each cell, including those diagonal, so initially you will see that 8 red squares around the white square are selected, as this is the default for the Game of Life.  You could define your own cellular automata that includes the next outer layer of neighbors by left clicking those blue squares to turn them red.  (Tip: for some really odd behavior, try a lopsided/assymetric neighborhood).

**Current**: Same as New, except it keeps existing neighborhood and rule definitions, instead of clearing them and resetting to the Game of Life definition.

**Done**: Click this after you finish defining your neighborhood.  You will then be asked one or two questions about your automata.  
  
  -"Is your automata Outer Totalistic?" is asking whether you want seperate distinct rules based on whether the current cell(center) is alive.  (I'm not sure my use of the term Outer Totalistic in this way is completely accurate). For the Game of Life you would choose Yes, as you need to be able to make the distinction that when 2 neighbors are alive, nothing changes, thus you need two different rules (If center is alive & two neighbors are alive, then result is alive) (If center is dead & two neighbors are alive, then result is dead).  If you choose No to this question, then whether the center is alive or not is not a distinct condition, in other words the rule can be no more complicated than (If 2 neighboring cells are alive, then the cell is dead.) which doesn't provide enough complexity to model Conway's Game of Life.

  -"Is the center cell part of the neighborhood?" is asked only if you choose No to the above question, and is essentially asking if when counting up the total number of alive cells in the neighborhood, if the current center cell should contribute to that count.

  -You are then presented with a dialog that lists all the possible scenarios for a cell in your automata.  For each, you select the checkbox on the right if that scenario should result in life for the cell.  So checking the box next to "Neighbors alive: 3" indicates that if a cell has 3 cells in its neighborhood which are alive, then it to should be alive in the next time step(perhaps it was already alive, and hence the rule ensures it remains alive, or it was dead and will become alive in the next time step).

  -The three disabled box at the top simply show you the properties of the neural network that will be generated based on your selected rules.
  
  -"Neighbor Link Weight": The link weights between neighboring neurons in the generated neural network(neighboring being defined by the neighborhood you defined previously)

  -"Center Link Weight": If greater than 0, indicates that eveery neuron will have a link to itself, i.e. if it is in a firing state, then in the next time step it will contribute that much to it's own input signal.

  -"Activation Values": When calculating the next time step, all input signals must add to one of these amounts for the current neuron to fire.
  
The magic of cellular automata is in the way that you get a system that appears to be relatively complex, from fairly simple rules.  I liken this to the complexity of an ant colony, despite the relative simplicity of the individual ants(although in defense of the ant, it does have 250,000 neurons).


