#genepoolかほしい桁のsliderを用意する
ary1 = [i for i in range(len(x))]
out = []
for j in range(len(x)):
    print(x)
    temp = x.pop(0)
    print(temp)
    print(ary1[temp])
    out.append(ary1.pop(temp))
    roop = 0;
    for k in x:
        if temp <= k:
          x[roop] = x[roop] - 1
        roop +=1
a=out
