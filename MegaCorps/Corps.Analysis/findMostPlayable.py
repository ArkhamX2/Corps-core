from spire.xls import *
from spire.xls.common import *
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import numpy as np
import multiprocessing

def test(path):
    # prepare
    wb = Workbook()
    wb.LoadFromFile(path)
    ignoreSheets = []
    ignoreResults = [0]
    playerId = 0
    totalGames = int(wb.Worksheets[0].AllocatedRange.Rows[0].Columns[7].Value)
    totalCards = int(wb.Worksheets[0].AllocatedRange.Rows[0].Columns[1].Value)
    baseSheet = 0
    minTurnCount = 8
    graphCount = 0
    names = []
    counter = 0
    while(True):
        for r in wb.Worksheets[counter].AllocatedRange.Columns[0]:
            if (r.Value==''):
                graphCount+=1
        for r in wb.Worksheets[counter].AllocatedRange.Columns[1]:
            if (r.Value!='' and not(r.Value.isdigit())):
                names.append(r.Value)            
        names = [ ' '.join(x) for x in zip(names[0::2], names[1::2]) ]
        graphs = []
        for i in range(graphCount):
            graphs.append([])
        if (len(graphs)!=0):
            break
        else:
            counter+=1
    # read wb
    try:
        for s in range(len(wb.Worksheets)):
            if (s in ignoreSheets):
                pass
            else:
                textList = []
                sheet = wb.Worksheets[s]
                locatedRange = sheet.AllocatedRange
                for i in range(len(sheet.Rows)):
                    st = ""
                    for j in range(len(locatedRange.Rows[i].Columns)):
                        st += (locatedRange[i + 1, j + 1].Value + "  ")
                    textList.append(st)
                botScore = []
                botScore.append([])
                j=0
                c = 0
                botScore[j].append(c)
                x = float(int(textList[0].split("  ")[3])/totalCards*100)
                y = float(int(textList[0].split("  ")[5])/totalCards*100)
                if (len(textList) > 1):
                    for i in range(2, len(textList)):
                        if ((textList[i].split("  ")[0]=="Strategy")):
                            botScore[j].append((int(textList[i+1].split("  ")[1])))
                        else:
                            if ((textList[i].split("  ")[0]=="TurnCount")):
                                if (int(textList[i].split("  ")[1]) <= minTurnCount):
                                    botScore.pop()
                                    j = j - 1
                                else:
                                    pass
                            else:
                                if ((textList[i].split("  ")[0]=="")):
                                    j = j + 1
                                    c = c + 1
                                    botScore.append([])
                                    botScore[j].append(c)
                    for i, score in enumerate(botScore):
                        graphs[score[0]].append((round(x,2),round(y,2),round(score[playerId + 1]/(sum(score)-score[0])*100,2)))
                else:
                    pass
    except:
        pass
    # draw plots
    for r in ignoreResults:
        graphs.remove(graphs[r])
    ze = []
    
    for graph in graphs:
        z = {}
        for g in graph:
            try:
                z[(g[0],g[1])]+=g[2]
            except:
                z[(g[0],g[1])]=g[2]
        ze.append(z)
    print(path + " finished")
    return ze

def fill(deckSize):
    a=[]
    max = deckSize
    for i in range(1,max,1):
        for j in range(1, max-i,1):
            a.append((i,j))
    x = []
    y = []
    for t in a:
        x.append(round(t[0]/deckSize*100,2))
        y.append(round(t[1]/deckSize*100,2))
    return x,y

def tm():
    awg = False    
    deckSize = 100
    wbs = ["./SimResultsRaN.xlsx","./SimResultsAN.xlsx","./SimResultsDN.xlsx","./SimResultsReN.xlsx","./SimResultsCN.xlsx"]
    #wbs = ["./SimResultsCN.xlsx"]
    # ,"./SimResultsAN.xlsx","./SimResultsDN.xlsx","./SimResultsReN.xlsx","./SimResultsCN.xlsx"
    pool_obj = multiprocessing.Pool() 
    graphs = []
    a = []
    a.extend(pool_obj.map(test, wbs))
    for p in a:
        graphs.extend(p)
    #graphs = []
    #graphs.extend(test(wbs[0]))
    if (awg):    
        res = []    
        for g in graphs:
            c = 0
            sum:float = 0
            for i in range(1,deckSize,1):
                for j in range(1, deckSize-i,1):
                    x = round(i/deckSize*100,2)
                    y = round(j/deckSize*100,2)
                    try:
                        sum += float(g[x,y])
                    except:
                        pass
            res.append(round(sum/len(g),2))
        for r in res:
            print(r)
    else:
        maxChance = 0
        rex = 0
        rey = 0
        for i in range(1,deckSize,1):
            for j in range(1, deckSize-i,1):
                sum:float = 0
                x = round(i/deckSize*100,2)
                y = round(j/deckSize*100,2)
                for g in graphs:                  
                    try:
                        sum += float(g[x,y])
                    except:
                        pass 
                sum = sum/len(graphs)
                if (sum > maxChance):
                    maxChance = sum
                    rex = x
                    rey = y
        print(maxChance)
        print(rex)
        print(rey)

if __name__ == '__main__':
    tm()