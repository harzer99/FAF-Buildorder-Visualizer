# FAF Buildorder Visualizer
 A tool to get an intuiton for good build orders

Buildorder is defined in bo.txt. Defined buildings are landfac, airfac, pgen, mex, hydro. Format: <br>

**building,buildpower** <br>
factory,10 <br>
pgen,10 <br>
pgen,10 <br>

expansion buildings are defined <br>
**timestamp,type,massreclaim,energyreclaim**<br>
100,mex,0,0<br>
120,reclaim,10,50<br>
145,hydro,0,0<br>

Run the program via FAF-Buildorder-Visualizer\bin\Debug\net7.0\app.exe. It should open a browser window with an interactive plot

Special thanks to my favorite Zoomer Grimplex for working with me on this project and showing me C# B)
