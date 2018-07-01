#pragma once
#include <String>
#include "votingsystem.h"

// Although this class is called MMPVotingSystem, it includes a special case for Supplementary Member.

enum MMPOptions
{
	AllowOverhang,
	NoOverhang,
	NonAward,
	SupplementaryMember,
	Proportional,
};

struct MMPVote
{
	int localVote;
	int partyVote;
};

struct MMPCandidate
{
	int partyIndex;
	int votes;
	bool isExcluded;
	map<String, String>* properties;
};

struct MMPElectorate
{
	int candidateCount;
	int winnerIndex;
	MMPCandidate* candidates;
	map<String, String>* properties;
	vector<MMPVote>* votes;
};

struct MMPParty
{
	bool isElected;
	String partyName;
	int listSeats;
	int partyVotes;
	vector<int>* electorateSeatWinners;
};

class MMPVotingSystem :
	public VotingSystem
{
public:
	MMPVotingSystem(MMPOptions overhang);
	~MMPVotingSystem(void);
	int MMPVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData);
	int MMPVotingSystem::PrintResults();
	vector<int>* MMPVotingSystem::WithPreferenceOrders(WithPreferenceOrdersInfo info);
	vector<int>* MMPVotingSystem::CalculateAdditional(vector<int>* currentSeatCount, vector<double>* nationalSupport);
private:
	int electorateCount;
	MMPElectorate* electorates;
	MMPOptions overhang;
	double threshold;
	int seatCount;
	int finalSeatCount;
	double smAmount;
	vector<MMPParty>* parties;
};

