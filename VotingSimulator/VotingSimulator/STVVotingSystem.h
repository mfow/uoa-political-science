#pragma once
#include "votingsystem.h"

enum STVTransferMethod
{
	InstantTransfer,
	FractionalTransfer,
	RandomTransfer,
	StackTransfer,
	QueueTransfer,
};

struct STVVote
{
	int* votes;
	int lastVote;
	double weight;
};

struct STVCandidate
{
	int partyIndex;
	double votes;
	bool isExcluded;
	bool isElected;
	map<String, String>* properties;
};

struct STVElectorate
{
	int candidateCount;
	double quota;
	int seatCount;
	STVCandidate* candidates;
	map<String, String>* properties;
	vector<STVVote>* votes;
};

class STVVotingSystem :
	public VotingSystem
{
public:
	STVVotingSystem(STVTransferMethod transferMethod);
	~STVVotingSystem(void);
	int STVVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData);
	int STVVotingSystem::PrintResults();
	vector<int>* STVVotingSystem::WithPreferenceOrders(WithPreferenceOrdersInfo info);
private:
	STVTransferMethod transferMethod;
	int electorateCount;
	STVElectorate* electorates;
	vector<int>* STVVotingSystem::calculateElectorateWinners(STVElectorate electorate);
};

