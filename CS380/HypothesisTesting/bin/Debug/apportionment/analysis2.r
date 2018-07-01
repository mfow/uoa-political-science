path = "C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/apportionment/"

hamilton.df =	read.table(paste(path,"hamilton.csv",sep=""), sep = ",", skip = 0, header=T)
hill.df =		read.table(paste(path,"hill.csv",sep=""), sep = ",", skip = 0, header=T)
jefferson.df =	read.table(paste(path,"jefferson.csv",sep=""), sep = ",", skip = 0, header=T)
stlague.df =	read.table(paste(path,"stlague.csv",sep=""), sep = ",", skip = 0, header=T)

hamilton.df$apportionment = "hamilton";
hill.df$apportionment = "hill";
jefferson.df$apportionment = "jefferson";
stlague.df$apportionment = "a_stlague";

data.df = hamilton.df;
data.df = merge(data.df, stlague.df, all=TRUE);
data.df = merge(data.df, jefferson.df, all=TRUE);
data.df = merge(data.df, hill.df, all=TRUE);
data.df$apportionment = factor(data.df$apportionment);

data.df$fairness = data.df$gallagher;
data.df$stability = data.df$enp-1;

fitFairness = lm(fairness~apportionment,data=data.df);
fitStability = lm(stability~apportionment,data=data.df);
anova(fitFairness)
anova(fitStability)

summary(fitFairness)
summary(fitStability)

confint(fitFairness)
confint(fitStability)