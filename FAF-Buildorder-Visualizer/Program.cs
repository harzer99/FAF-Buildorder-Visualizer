
using System.IO;
using XPlot.Plotly;
using Microsoft.Data.Analysis;
using System.Linq;
using Microsoft.AspNetCore.Html;

static string[,] parse_bo(string file)
{
    int i= 0;
    var lineCount = File.ReadLines(file).Count();
    string[,] parsed_bo = new string[lineCount,2];
    using(var reader = new StreamReader(file))
    {
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] values = line.Split(',');
            parsed_bo[i,0] = values[0];
            parsed_bo[i,1] = values[1];
            i +=1;
        }
    }
    return parsed_bo;
}

static string[,] parse_expansion(string file)
{
    int i= 0;
    var lineCount = File.ReadLines(file).Count();
    string[,] parsed_expansion = new string[lineCount,4];
    using(var reader = new StreamReader(file))
    {
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] values = line.Split(',');
            parsed_expansion[i,0] = values[0];
            parsed_expansion[i,1] = values[1];
            parsed_expansion[i,2] = values[2];
            parsed_expansion[i,3] = values[3];
            i +=1;
        }
    }
    return parsed_expansion;
}

static void expansions_in_timeframe(ref string[,] current_expansion, ref double delta_t, ref dynamic time, ref dynamic mass, ref dynamic energy, ref dynamic mass_income, ref dynamic energy_income)
{
    for (int i = 0; i < current_expansion.GetLength(0); i++)
    {
        if(time-delta_t <= double.Parse(current_expansion[i,0]) && double.Parse(current_expansion[i,0]) <time)
        {
            if(current_expansion[i,1] == "mex")
            {
                mass_income += 2;
                energy_income -= 2;
                mass -= 36;
                energy -=360;
            }
            else if(current_expansion[i,1] == "hydro")
            {
                energy_income += 100;
                mass -= 160;
                energy -=800;

            }
            else if(current_expansion[i,1] == "reclaim")
            {
                mass += double.Parse(current_expansion[i,2]);
                energy += double.Parse(current_expansion[i,3]);
            }
        }
    }
}

static void iterate_simulation(ref DataFrame df, ref int current_step, ref string[,] current_bo, ref string[,] current_expansion, ref Dictionary<string, int[]> Buildings)
{
    string object_built = current_bo[current_step,0];
    double current_bp = double.Parse(current_bo[current_step,1]);
    double delta_t = Buildings[object_built][2]/current_bp;
    
    dynamic time = df["Time"][current_step];
    dynamic mass = df["Mass"][current_step];
    dynamic energy = df["Energy"][current_step];
    dynamic mass_income = df["Mass Income"][current_step];
    dynamic energy_income = df["Energy Income"][current_step];
    dynamic n_landfacs = df["landfacs"][current_step];
    dynamic n_airfacs = df["airfacs"][current_step];
    dynamic mass_units = df["Mass in Units"][current_step];
    
    
    mass += mass_income*delta_t;
    energy += energy_income*delta_t;
    mass -= Buildings[object_built][1];
    energy -= Buildings[object_built][0];
    time += delta_t;
    mass_units += delta_t*(3.5*n_landfacs+2.5*n_airfacs);
    
    if(object_built == "mex")
    {
        mass_income += 2;
        energy_income -=2;
    }
    else if(object_built == "pgen")
    {
        energy_income += 20;
    }
    else if(object_built == "hydro")
    {
        energy_income += 100;
    }
    else if(object_built =="landfac")
    {
        mass_income -= 3.5;
        energy_income -=17.5;
        n_landfacs += 1;
    }
    else if(object_built =="airfac")
    {
        mass_income -=2.5;
        energy_income -=50;
        n_airfacs +=1;
    }

    expansions_in_timeframe(ref current_expansion, ref delta_t, ref time, ref mass, ref energy, ref mass_income, ref energy_income);
    Console.WriteLine(string.Format("{0,-8} {1,-10} {2,-8} {3,-8} {4,-6}",Math.Round(time) , object_built, Math.Round(mass), Math.Round(energy), Math.Round(mass_units)));
    df.Append(new List<KeyValuePair<string, object>>() { 
    new KeyValuePair<string, object>("Time", time),
    new KeyValuePair<string, object>("Building", object_built),
    new KeyValuePair<string, object>("Mass", mass),
    new KeyValuePair<string, object>("Energy", energy),
    new KeyValuePair<string, object>("Mass Income", mass_income),
    new KeyValuePair<string, object>("Energy Income", energy_income),
    new KeyValuePair<string, object>("landfacs", n_landfacs),
    new KeyValuePair<string, object>("airfacs", n_airfacs),
    new KeyValuePair<string, object>("Mass in Units", mass_units)
    }, true);
}

string path_bo = @"bo.txt";
string path_expansion = @"expansion.txt";
string[,] current_bo = parse_bo(path_bo);
string[,] current_expansion = parse_expansion(path_expansion);


//defining starting variables
int current_step = 0;
double[] times = {5};
string[] buildings  ={""};
double[] masses ={650}; 
double[] energies = {4000}; 
double[] mass_incomes = {1};
double[] energy_incomes = {20};
int[] n_landfacs = {0};
int[] n_airfacs = {0}; 
double[] mass_units = {0};

//initalizing dataframe
DataFrameColumn[] columns = {
    new PrimitiveDataFrameColumn<double>("Time", times ),
    new StringDataFrameColumn("Building", buildings),
    new PrimitiveDataFrameColumn<double>("Mass", masses),
    new PrimitiveDataFrameColumn<double>("Energy", energies),
    new PrimitiveDataFrameColumn<double>("Mass Income", mass_incomes),
    new PrimitiveDataFrameColumn<double>("Energy Income", energy_incomes),
    new PrimitiveDataFrameColumn<int>("landfacs", n_landfacs),
    new PrimitiveDataFrameColumn<int>("airfacs", n_landfacs),
    new PrimitiveDataFrameColumn<double>("Mass in Units", mass_units)
};
DataFrame df = new(columns);

Dictionary<string, int[]> Buildings = new Dictionary<string, int[]>()
{
    // {Energycost, Masscost, Buildtime}
    {"landfac", new int[] {2100, 240, 300} },
    {"airfac", new int[] {2400, 210 , 300} },
    {"pgen", new int[] {750, 75, 125} },
    {"hydro", new int[] {800, 160, 400} },
    {"mex", new int[] {360, 36, 60} },
};
Console.WriteLine(string.Format("{0,-8} {1,-10} {2,-8} {3,-8} {4,-6}","time" , "building", "mass", "energy", "mass in units"));
for (int i = 0; i < current_bo.GetLength(0); i++)
{
    iterate_simulation(ref df, ref current_step, ref current_bo, ref current_expansion, ref Buildings);
    current_step += 1;
}
//Console.WriteLine(df);

var chart2_list = new List<Scatter> 
{
    new Scatter
    {
         x = df.Columns["Time"],
        y = df.Columns["Mass"],
        name="Mass",
        mode = "lines"
    },
    new Scatter    
    {       
        x = df.Columns["Time"],
        y = df.Columns["Energy"],
        name="Energy",
        mode = "lines"
    },
    new Scatter    
    {       
        x = df.Columns["Time"],
        y = df.Columns["Mass in Units"],
        name="Mass in Units",
        mode = "lines"
    }
    
};
 
var chart2 = Chart.Plot(
    chart2_list
);
 
var chart2_layout = new Layout.Layout{
    title="Buildorder",
    xaxis =new Xaxis{
        title = "Time"
        },
    yaxis =new Yaxis{
    title = "Resources"
        }           
    };
chart2.WithLayout(chart2_layout);
chart2.Show();