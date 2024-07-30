from spire.xls import *
from spire.xls.common import *
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import numpy as np

def test():
    ignoreSheets = []
    ignoreResults = [0]
    playerId = 0
    totalGames = 10
    totalCards = 100
    baseSheet = 0
    graphCount = 6
    graphs = []
    for i in range(graphCount):
        graphs.append([])
    wb = Workbook()
    wb.LoadFromFile("./SimResults.xlsx")
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
            x = float(int(textList[0].split("  ")[3])/totalCards*100)
            y = float(int(textList[0].split("  ")[5])/totalCards*100)
            if (len(textList) > 1):
                for i in range(2, len(textList)):
                    if ((textList[i].split("  ")[0]=="Strategy")):
                        botScore[j].append((int(textList[i+1].split("  ")[1])))
                    else:
                        if ((textList[i].split("  ")[0]=="")):
                            j = j + 1
                            botScore.append([])
                for i, score in enumerate(botScore):
                    graphs[i].append((round(x,2),round(y,2),round(score[playerId]/(sum(score))*100,2)))
            else:
                pass

    x,y = fill(totalCards)
    xgrid, ygrid = np.meshgrid(x, y)
    for i, graph in enumerate(graphs):
        if (i in ignoreResults):
            pass
        else:
            z = {}
            for g in graph:
                z[(g[0],g[1])]=g[2]
            zgrid = []
            for j in range(len(xgrid)):
                zgrid.append([])

            for k in range(len(zgrid)):
                for l in range(len(xgrid[k])):
                    try:
                        zgrid[k].append(z[(xgrid[k][l],ygrid[k][l])])
                    except:
                        zgrid[k].append(np.NaN)
            fig, ax = plt.subplots()
            vmin = 0
            vmax = 100
            contourf_ = ax.contourf(xgrid, ygrid, zgrid, levels=np.linspace(vmin,vmax,400),extend='max')
            cbar = fig.colorbar(contourf_,ticks=range(vmin, vmax+1, 25))

            fig.show()
    input()

def fill(deckSize):
    a=[]
    max = deckSize
    for i in range(7,max,7):
        for j in range(10, max-i,10):
            a.append((i,j))
    x = []
    y = []
    for t in a:
        x.append(t[0])
        y.append(t[1])
    return x,y
test()