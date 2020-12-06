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
  private void RunScript(string FilePath, Point3d origin, double Scale, ref object Crv, ref object Pt, ref object Type)
  {
        //xmlファイルを指定する
    XElement xml = XElement.Load(FilePath);
    XNamespace ns = xml.Name.Namespace;

    XNamespace ns_gml = "http://www.opengis.net/gml/3.2";
    //↑二つ目以降のnsの取得が分からん、gml

    //cntrタグを探索
    IEnumerable<XElement> infos = from item in xml.Elements(ns + "Cntr")
        select item;

    //リスト作成
    List<Point3d> ContaPt = new List<Point3d>();
    List<Polyline> ContaCrv = new List<Polyline>();
    List<String> TypeStr = new List<String>();


    //cntrごとにループして、コンタのCrvとタイプを処理
    foreach (XElement info in infos)
    {
      XNamespace ns_info = info.Name.Namespace;

      //コンタの高さを取得
      IEnumerable<XElement> elems_alti = info.Descendants(ns_info + "alti");
      float conta_zpt = 0;
      foreach (XElement elem_alti in elems_alti)
      {
        conta_zpt = float.Parse(elem_alti.Value);
      }

      //タイプを取得
      IEnumerable<XElement> elems_type = info.Descendants(ns_info + "type");
      foreach (XElement elem_type in elems_type)
      {
        Print(elem_type.Value);
      }

      //コンタの座標を取得
      IEnumerable<XElement> elems_posList = info.Descendants(ns_gml + "posList");
      foreach (XElement elem_posList in elems_posList)
      {
        StringReader elem_pos = new StringReader(elem_posList.Value);

        Polyline temp_crv = new Polyline();

        //ポイントごとに分割して処理
        while(true)
        {
          String line = elem_pos.ReadLine();

          if(line == string.Empty)
          {

          }
          else if(line != null)
          {
            string[] radxy = line.Split(' ');

            //スケールを調整
            float conta_ypt = float.Parse(radxy[0]);
            float conta_xpt = float.Parse(radxy[1]);
            conta_zpt = conta_zpt;

            Point3d temp_pt = new Point3d(conta_xpt, conta_ypt, conta_zpt);
            ContaPt.Add(temp_pt);


            temp_crv.Add(float.Parse(radxy[0]), float.Parse(radxy[1]), conta_zpt);
            //ContaCrv.Add(temp_crv);
          }
          else if(line == null)
          {
            break;
          }

        }
        Print(temp_crv.ToString());
        ContaCrv.Add(temp_crv);
      }
      //ContaCrv.Add(Polyline(ContaPt));
      //ContaCrv.Add(

    }

    Pt = ContaPt;
    Crv = ContaCrv;
  }

  // <Custom additional code> 
  

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
        string FilePath = default(string);
    if (inputs[0] != null)
    {
      FilePath = (string)(inputs[0]);
    }

    Point3d origin = default(Point3d);
    if (inputs[1] != null)
    {
      origin = (Point3d)(inputs[1]);
    }

    double Scale = default(double);
    if (inputs[2] != null)
    {
      Scale = (double)(inputs[2]);
    }



    //3. Declare output parameters
      object Crv = null;
  object Pt = null;
  object Type = null;


    //4. Invoke RunScript
    RunScript(FilePath, origin, Scale, ref Crv, ref Pt, ref Type);
      
    try
    {
      //5. Assign output parameters to component...
            if (Crv != null)
      {
        if (GH_Format.TreatAsCollection(Crv))
        {
          IEnumerable __enum_Crv = (IEnumerable)(Crv);
          DA.SetDataList(1, __enum_Crv);
        }
        else
        {
          if (Crv is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(Crv));
          }
          else
          {
            //assign direct
            DA.SetData(1, Crv);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }
      if (Pt != null)
      {
        if (GH_Format.TreatAsCollection(Pt))
        {
          IEnumerable __enum_Pt = (IEnumerable)(Pt);
          DA.SetDataList(2, __enum_Pt);
        }
        else
        {
          if (Pt is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(2, (Grasshopper.Kernel.Data.IGH_DataTree)(Pt));
          }
          else
          {
            //assign direct
            DA.SetData(2, Pt);
          }
        }
      }
      else
      {
        DA.SetData(2, null);
      }
      if (Type != null)
      {
        if (GH_Format.TreatAsCollection(Type))
        {
          IEnumerable __enum_Type = (IEnumerable)(Type);
          DA.SetDataList(3, __enum_Type);
        }
        else
        {
          if (Type is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(3, (Grasshopper.Kernel.Data.IGH_DataTree)(Type));
          }
          else
          {
            //assign direct
            DA.SetData(3, Type);
          }
        }
      }
      else
      {
        DA.SetData(3, null);
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