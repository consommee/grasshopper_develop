    List<Vector3d> move = new List<Vector3d>();
    foreach(Point3d temp_point in points)
    {
      Vector3d temp_vector = new Vector3d((temp_point.X - item_origin.X), (temp_point.Y - item_origin.Y), (temp_point.Z - item_origin.Z));
      move.Add(temp_vector);
    }

    int step = 0;
    DataTree<GeometryBase> output = new DataTree<GeometryBase>();
    foreach(Vector3d temp_vector in move)
    {

      Transform movement = Transform.Translation(temp_vector);

      GH_Path ghp = new GH_Path(step);

      foreach(GeometryBase geo in item)
      {
        GeometryBase geo_temp = geo.Duplicate();
        geo_temp.Transform(movement);
        output.Add(geo_temp, ghp);
        geo_temp = null;
      }
      step = step + 1;

    }
    Print(output.ToString());
    A = output;
