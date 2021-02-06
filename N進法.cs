//インプット　int inputNum, int N, int digid, ref object A

//inputNumの範囲をoutへ
    double slidermax = Math.Pow(N, N) - 1;
    Print(0+" < "+slidermax);
    
    //リスト作成
    List < int > outList = new List<int>();
    int temp = 0;
    for (int i = 0; inputNum > 0; i++) {
      temp = 0;
      temp = inputNum % N;
      outList.Add(temp);
      inputNum = inputNum / N;
    }
    
    outList.Reverse();
    
    for(int j = 0 ; outList.Count() <= digid-1;)
    {
      outList.Insert(0,0);
    }
    A = outList;
    //A = outList.AsEnumerable().Reverse();