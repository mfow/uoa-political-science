PROC IMPORT OUT= WORK.nz08 
            DATAFILE= "C:\Users\Michael\Documents\Visual Studio 11\Proje
cts\CS380\Data\NZ\nzes08\2008NZES_Release.sav" 
            DBMS=SPSS REPLACE;

RUN;

DATA nz08_2;
SET nz08;

if zvt08p=. then DELETE;

NAT = znatlk;
if NAT="Strong dislike" then NAT=0;
if NAT="Neutral" then NAT=5;
if NAT="String Like" then NAT=10;
if NAT=99 then NAT=0;

LAB = zlablk;
if LAB="Strong dislike" then LAB=0;
if LAB="Neutral" then LAB=5;
if LAB="String Like" then LAB=10;
if LAB=99 then LAB=0;

NZF = znzflik;
if NZF="Strong dislike" then NZF=0;
if NZF="Neutral" then NZF=5;
if NZF="String Like" then NZF=10;
if NZF=99 then NZF=0;

UF = zuflik;
if UF="Strong dislike" then UF=0;
if UF="Neutral" then UF=5;
if UF="String Like" then UF=10;
if UF=99 then UF=0;

ACT = zactlik;
if ACT="Strong dislike" then ACT=0;
if ACT="Neutral" then ACT=5;
if ACT="String Like" then ACT=10;
if ACT=99 then ACT=0;

GRN = zgrnlik;
if GRN="Strong dislike" then GRN=0;
if GRN="Neutral" then GRN=5;
if GRN="String Like" then GRN=10;
if GRN=99 then GRN=0;

MAO = zmaolik;
if MAO="Strong dislike" then MAO=0;
if MAO="Neutral" then MAO=5;
if MAO="String Like" then MAO=10;
if MAO=99 then MAO=0;

PRO = zprglik;
if PRO="Strong dislike" then PRO=0;
if PRO="Neutral" then PRO=5;
if PRO="String Like" then PRO=10;
if PRO=99 then PRO=0;

*zvt08p voted for 2008;
*zvot05p voted for 2005;
*ZZWT6 weight;

KEEP zvt08p zvot05p ZZWT6 NAT LAB NZF UF ACT GRN MAO PRO;

RENAME zvt08p=VOTE2008;
RENAME zvot05p=VOTE2005;
RENAME ZZWT6=WEIGHT;

RUN;

PROC CONTENTS DATA=nz08_2; 
RUN; 

PROC EXPORT DATA= WORK.NZ08_2 
            OUTFILE= "C:\Users\Michael\Documents\Visual Studio 11\Projec
ts\CS380\Data\NZ\nzpref.csv" 
            DBMS=CSV LABEL REPLACE;
     PUTNAMES=YES;
RUN;
