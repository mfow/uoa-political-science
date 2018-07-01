hyp.df <- read.table("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/HypothesisTesting/bin/Debug/enisim.csv", sep = ",", skip = 0, header=T)
hyp.df$X <- NULL
hyp.df[1:5,]

hyp.df$roundfactor = factor(hyp.df$Round)

fit1 = lm(Loosemorehanby~roundfactor, data=hyp.df)
fit2 = lm(Gallagher~roundfactor, data=hyp.df)
fit3 = lm(EffectiveNumberOfParties~roundfactor, data=hyp.df)
fit4 = lm(EffectiveNumberOfPCAVars~roundfactor, data=hyp.df)
fit5 = lm(Governability~roundfactor, data=hyp.df)

anova(fit1)
anova(fit2)
anova(fit3)
anova(fit4)
anova(fit5)

summary(fit1)




unique(hyp.df$ENI)




set1 = hyp.df[hyp.df$Round == 1,]
r = rnorm(nrow(set1))
set1a = set1[r>0,]
set1b = set1[r<0,]
nrow(set1a)
nrow(set1b)
t.test(set1a$Loosemorehanby,set1b$Loosemorehanby)

