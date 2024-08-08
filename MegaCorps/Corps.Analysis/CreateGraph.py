from spire.xls import *
from spire.xls.common import *
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import numpy as np

def test():
    # prepare
    wb = Workbook()
    wb.LoadFromFile("./SimResultsA.xlsx")
    ignoreSheets = []
    ignoreResults = [0]
    playerId = 0
    totalGames = int(wb.Worksheets[0].AllocatedRange.Rows[0].Columns[7].Value)
    totalCards = int(wb.Worksheets[0].AllocatedRange.Rows[0].Columns[1].Value)
    baseSheet = 0
    graphCount = 0
    names = []
    for r in wb.Worksheets[0].AllocatedRange.Columns[0]:
        if (r.Value==''):
            graphCount+=1
    for r in wb.Worksheets[0].AllocatedRange.Columns[1]:
        if (r.Value!='' and not(r.Value.isdigit())):
            names.append(r.Value)            
    names = [ ' '.join(x) for x in zip(names[0::2], names[1::2]) ]
    graphs = []
    for i in range(graphCount):
        graphs.append([])
    # read wb
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
    # draw plots
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
            num = 100
            contourf_ = ax.contourf(xgrid, ygrid, zgrid, levels=np.linspace(vmin,vmax,num),extend='max')
            cbar = fig.colorbar(contourf_,ticks=range(vmin, vmax+1, 25))
            t = fig.get_axes()[0].axes
            t.set_xlabel("Attack cards (%)")
            t.set_ylabel("Defence cards (%)")
            a,b = names[i].split(" ")
            t.title.set_text(a + " vs " + b)
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
        x.append(round(t[0]/deckSize*100,2))
        y.append(round(t[1]/deckSize*100,2))
    return x,y
test()