using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;

using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { __out.Add(text); }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private IGH_Component Component; 
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments, 
  /// Output parameters as ref arguments. You don't have to assign output parameters, 
  /// they will have a default value.
  /// </summary>
  private void RunScript(double x, double y, double x_origin, double y_origin, ref object aa, ref object bb)
  {
        //参考　https://qiita.com/sw1227/items/e7a590994ad7dcd0e8ab
    
    //緯度経路をラジアンに
    double phi_rad = Math.PI * (y) / 180.0;
    double lambda_rad = Math.PI * (x) / 180.0;

    double phi0_rad = Math.PI * (y_origin) / 180.0;
    double lambda0_rad = Math.PI * (x_origin) / 180.0;

    //定数
    double m0 = 0.9999;
    double a = 6378137.0;
    double F = 298.257222101;

    double n = 1.0 / (2 * F - 1);
    double[] A_array = A(n, 6);
    double[] Alpha_array = Alpha(n);
    //double[] Deta_array = Beta(n);
    //double[] Delta_array = Delta(n);

    double A_ = ( (m0 * a) / (1.0 + n) ) * A_array[0];
    
    double[] one_six = {1,2,3,4,5};
    double[] A_array_first = A(n, 5);
    
    double[] Sin_temp = Sin_array(multiply_array(2 * phi0_rad, one_six));
    double[] sums = multiply(A_array_first, Sin_temp);
    double dots = sums.Sum();
    double S_ = ( (m0 * a) / (1.0 + n) ) * ( A_array[0] * phi0_rad +sums.Sum());

    double lambda_c = Math.Cos(lambda_rad - lambda0_rad);
    double lambda_s = Math.Sin(lambda_rad - lambda0_rad);
    
    double t = Math.Sinh(Atanh(Math.Sin(phi_rad)) - ((2 * Math.Sqrt(n)) / (1 + n)) * Atanh(((2 * Math.Sqrt(n)) / (1 + n)) * Math.Sin(phi_rad)));
    double t_ = Math.Sqrt(1 + t * t);
    
    double xi2 = Math.Atan(t / lambda_c);
    double eta2 = Atanh(lambda_s / t_); 
    //eta2 = 0.0035974580637255257;
    
    double[] Sin_temp1 = Sin_array(multiply_array(2 * xi2, one_six));
    double[] Cosh_temp = Cosh_array(multiply_array(2 * eta2, one_six));
    double[] x_multiply = multiply(Alpha_array, multiply(Sin_temp1, Cosh_temp));
    double result_x = A_ * (xi2 + x_multiply.Sum()) - S_;

    double[] Cos_temp = Cos_array(multiply_array(2 * xi2, one_six));
    double[] Sinh_temp = Sinh_array(multiply_array(2 * eta2, one_six));
    double[] y_multiply = multiply(Alpha_array, multiply(Cos_temp, Sinh_temp));
    double result_y = A_ * (eta2 + y_multiply.Sum());
    
    aa = result_x;
    bb = result_y;


  }

  // <Custom additional code> 
    public double[] A(double n, double i)
  {
    double A0 = 1 + Math.Pow(n, 2) / 4.0 + Math.Pow(n, 4) / 64.0;
    double A1 = -(3.0 / 2) * ( n - Math.Pow(n, 3) / 8.0 - Math.Pow(n, 5) / 64.0 );
    double A2 = (15.0 / 16) * (  Math.Pow(n, 2) - Math.Pow(n, 4) / 4.0 );
    double A3 = -(35.0 / 48) * (  Math.Pow(n, 3) - (5.0 / 16) * Math.Pow(n, 5) );
    double A4 = (315.0 / 512) * Math.Pow(n, 4);
    double A5 = -(693.0 / 1280) * Math.Pow(n, 5);

    if(i == 6)
    {
      double[] Alpah_array = {A0,A1,A2,A3,A4,A5};
      return Alpah_array;
    }
    else if (i == 5)
    {
      double[] Alpah_array = {A1,A2,A3,A4,A5};
      return Alpah_array;
    }
    else
    {
      double[] Alpah_array = {A0,A1,A2,A3,A4,A5};
      return Alpah_array;
    }
  }
  
  public double[]  Alpha(double n)
  {
    //double a0 = 0;
    double a1 = (1.0 / 2) * n - (2.0 / 3) * Math.Pow(n, 2) + (5.0 / 16) * Math.Pow(n, 3) + (41.0 / 180) * Math.Pow(n, 4) - (127.0 / 288) * Math.Pow(n, 5);
    double a2 = (13.0 / 48) * Math.Pow(n, 2) - (3.0 / 5) * Math.Pow(n, 3) + (557.0 / 1440) * Math.Pow(n, 4) + (281.0 / 630) * Math.Pow(n, 5);
    double a3 = (61.0 / 240) * Math.Pow(n, 3) - (103.0 / 140) * Math.Pow(n, 4) + (15061.0 / 26880) * Math.Pow(n, 5);
    double a4 = (49561.0 / 161280) * Math.Pow(n, 4) - (179.0 / 168) * Math.Pow(n, 5);
    double a5 = (34729.0 / 80640) * Math.Pow(n, 5);

    double[] Alpha_array = { a1, a2, a3, a4, a5};
    return Alpha_array;
  }

  public double[]  Beta(double n)
  {
    double B0 = 0;
    double B1 = (1.0 / 2) * n - (2.0 / 3) * Math.Pow(n, 2) + (37.0 / 96) * Math.Pow(n, 3) - (1.0 / 360) * Math.Pow(n, 4) - (81.0 / 512) * Math.Pow(n, 5);
    double B2 = (1.0 / 48) * Math.Pow(n, 2) + (1.0 / 15) * Math.Pow(n, 3) - (437.0 / 1440) * Math.Pow(n, 4) + (46.0 / 105) * Math.Pow(n, 5);
    double B3 = (17.0 / 480) * Math.Pow(n, 3) - (37.0 / 840) * Math.Pow(n, 4) - (209.0 / 4480) * Math.Pow(n, 5);
    double B4 = (4397.0 / 161280) * Math.Pow(n, 4) - (11.0 / 504) * Math.Pow(n, 5);
    double B5 = (4583.0 / 161280) * Math.Pow(n, 5);

    double[] Beta_array = {B0, B1, B2, B3, B4, B5};
    return Beta_array;
  }

  public double[]  Delta(double n)
  {
    double D0 = 0;
    double D1 = 2.0 * n - (2.0 / 3) * Math.Pow(n, 2) - 2.0 * Math.Pow(n, 3) + (116.0 / 45) * Math.Pow(n, 4) + (26.0 / 45) * Math.Pow(n, 5) - (2854.0 / 675) * Math.Pow(n, 6);
    double D2 = (7.0 / 3) * Math.Pow(n, 2) - (8.0 / 5) * Math.Pow(n, 3) - (227.0 / 45) * Math.Pow(n, 4) + (2704.0 / 315) * Math.Pow(n, 5) + (2323.0 / 945) * Math.Pow(n, 6);
    double D3 = (56.0 / 15) * Math.Pow(n, 3) - (136.0 / 35) * Math.Pow(n, 4) - (1262.0 / 105) * Math.Pow(n, 5) + (73814.0 / 2835) * Math.Pow(n, 6);
    double D4 = (4279.0 / 630) * Math.Pow(n, 4) - (332.0 / 35) * Math.Pow(n, 5) - (399572.0 / 14175) * Math.Pow(n, 6);
    double D5 = (4174.0 / 315) * Math.Pow(n, 5) - (144838.0 / 6237) * Math.Pow(n, 6);
    double D6 = (601676.0 / 22275) * Math.Pow(n, 6);

    double[] Delta_array = {D0, D1, D2, D3, D4, D5, D6};
    return Delta_array;
  }
  
  public double Atanh(double n)
  {
    return (Math.Log((1 + n) / (1 - n)) / 2);
    //return Math.Atanh(n);
  }
  
  public double[] multiply(double[] n, double[] m)
  {
    double[] multy_array = new double[n.Length]; 
    for (int i = 0; i < n.Length; i++)
    {
      multy_array[i] = n[i] * m[i];
    }
    return multy_array;
  }
  
  public double[] multiply_array(double n, double[] m)
  {
    double[] multy_array = new double[m.Length]; 
    for (int i = 0; i < m.Length; i++)
    {
      multy_array[i] = n * m[i];
    }
    return multy_array;
  }

  public double[] Sin_array(double[] m)
  {
    double[] multy_array = new double[m.Length]; 
    for (int i = 0; i < m.Length; i++)
    {
      multy_array[i] = Math.Sin(m[i]);
    }
    return multy_array;
  }
  
  public double[] Cosh_array(double[] m)
  {
    double[] multy_array = new double[m.Length]; 
    for (int i = 0; i < m.Length; i++)
    {
      multy_array[i] = Math.Cosh(m[i]);
    }
    return multy_array;
  }
  
  public double[] Cos_array(double[] m)
  {
    double[] multy_array = new double[m.Length]; 
    for (int i = 0; i < m.Length; i++)
    {
      multy_array[i] = Math.Cos(m[i]);
    }
    return multy_array;
  }
  
  public double[] Sinh_array(double[] m)
  {
    double[] multy_array = new double[m.Length]; 
    for (int i = 0; i < m.Length; i++)
    {
      multy_array[i] = Math.Sinh(m[i]);
    }
    return multy_array;
  }
  
  public double[] ArrayPrint(double[] m)
  {
    double[] multy_array = new double[m.Length]; 
    Print("[");
    for (int i = 0; i < m.Length; i++)
    {
      Print(m[i].ToString());
    }
    Print("]");
    return multy_array;
  }
  
  public double dot(double[] m)
  {
    double multy_array = 1; 
    for (int i = 0; i < m.Length; i++)
    {
      multy_array = multy_array * m[i];
    }
    return multy_array;
  }
  /// <summary>
  /// This method will be called once every solution, before any calls to RunScript.
  /// </summary>
  public override void BeforeRunScript()
  { }
  /// <summary>
  /// This method will be called once every solution, after any calls to RunScript.
  /// </summary>
  public override void AfterRunScript()
  { }

  // </Custom additional code> 

  private List<string> __err = new List<string>(); //Do not modify this list directly.
  private List<string> __out = new List<string>(); //Do not modify this list directly.
  private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
  private IGH_ActiveObject owner;                  //Legacy field.
  private int runCount;                            //Legacy field.
  
  public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
  {
    //Prepare for a new run...
    //1. Reset lists
    this.__out.Clear();
    this.__err.Clear();

    this.Component = owner;
    this.Iteration = iteration;
    this.GrasshopperDocument = owner.OnPingDocument();
    this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

    this.owner = this.Component;
    this.runCount = this.Iteration;
    this. doc = this.RhinoDocument;

    //2. Assign input parameters
        double x = default(double);
    if (inputs[0] != null)
    {
      x = (double)(inputs[0]);
    }

    double y = default(double);
    if (inputs[1] != null)
    {
      y = (double)(inputs[1]);
    }

    double x_origin = default(double);
    if (inputs[2] != null)
    {
      x_origin = (double)(inputs[2]);
    }

    double y_origin = default(double);
    if (inputs[3] != null)
    {
      y_origin = (double)(inputs[3]);
    }



    //3. Declare output parameters
      object aa = null;
  object bb = null;


    //4. Invoke RunScript
    RunScript(x, y, x_origin, y_origin, ref aa, ref bb);
      
    try
    {
      //5. Assign output parameters to component...
            if (aa != null)
      {
        if (GH_Format.TreatAsCollection(aa))
        {
          IEnumerable __enum_aa = (IEnumerable)(aa);
          DA.SetDataList(1, __enum_aa);
        }
        else
        {
          if (aa is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(aa));
          }
          else
          {
            //assign direct
            DA.SetData(1, aa);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }
      if (bb != null)
      {
        if (GH_Format.TreatAsCollection(bb))
        {
          IEnumerable __enum_bb = (IEnumerable)(bb);
          DA.SetDataList(2, __enum_bb);
        }
        else
        {
          if (bb is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(2, (Grasshopper.Kernel.Data.IGH_DataTree)(bb));
          }
          else
          {
            //assign direct
            DA.SetData(2, bb);
          }
        }
      }
      else
      {
        DA.SetData(2, null);
      }

    }
    catch (Exception ex)
    {
      this.__err.Add(string.Format("Script exception: {0}", ex.Message));
    }
    finally
    {
      //Add errors and messages... 
      if (owner.Params.Output.Count > 0)
      {
        if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
        {
          List<string> __errors_plus_messages = new List<string>();
          if (this.__err != null) { __errors_plus_messages.AddRange(this.__err); }
          if (this.__out != null) { __errors_plus_messages.AddRange(this.__out); }
          if (__errors_plus_messages.Count > 0) 
            DA.SetDataList(0, __errors_plus_messages);
        }
      }
    }
  }
}