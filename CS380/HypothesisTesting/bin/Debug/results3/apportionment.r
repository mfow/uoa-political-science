stlague.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/results3/apportionmentStLague_10_stv.csv", sep = ",", skip = 0, header=T)
hill.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/results3/apportionmentHill_10_stv.csv", sep = ",", skip = 0, header=T)
hamilton.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/results3/apportionmentHamilton_10_stv.csv", sep = ",", skip = 0, header=T)

windows()
plot.new()
hist(stlague.df$gallagher)

windows()
plot.new()
hist(stlague.df$enp)

windows()
plot.new()
hist(hill.df$gallagher)

windows()
plot.new()
hist(hill.df$enp)

windows()
plot.new()
hist(hamilton.df$gallagher)

windows()
plot.new()
hist(hamilton.df$enp)

t.test(stlague.df$gallagher, hill.df$gallagher)
t.test(stlague.df$gallagher, hamilton.df$gallagher)
t.test(stlague.df$enp, hill.df$enp)
t.test(stlague.df$enp, hamilton.df$enp)

