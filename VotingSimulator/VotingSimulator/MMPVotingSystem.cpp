#include "StdAfx.h"
#include "MMPVotingSystem.h"
#include "CSVReader.h"
#include "FPPVotingSystem.h"

MMPVotingSystem::MMPVotingSystem(MMPOptions overhang)
{
	this->overhang = overhang;

	if (overhang == MMPOptions::SupplementaryMember)
		smAmount = 0.75;
}


MMPVotingSystem::~MMPVotingSystem(void)
{
}

int MMPVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData)
{
	this->seatCount = _wtoi(customMetaData->at(L"SeatCount").c_str());
	this->threshold = _wtof(customMetaData->at(L"Threshold").c_str());
	this->finalSeatCount = this->seatCount; // We may increase this later.

	this->parties = new vector<MMPParty>;

	vector<String>* partyHeaders = readCSVLine(in);
	int partyNameHeaderIndex = getIndexOf(partyHeaders, L"Name");

	for (int h = 0; h < partyCount; h++)
	{
		vector<String>* partyData = readCSVLine(in);

		MMPParty party;

		party.listSeats = 0;
		party.electorateSeatWinners = new vector<int>;
		party.partyName = partyData->at(partyNameHeaderIndex);
		party.partyVotes = 0;
		party.isElected = false;

		this->parties->push_back(party);

		delete partyData;
	}

	delete partyHeaders;

	this->electorateCount = _wtoi(readSingle(in).c_str());
	this->electorates = new MMPElectorate[MMPVotingSystem::electorateCount];

	for (int i = 0; i < MMPVotingSystem::electorateCount; i++)
	{
		MMPElectorate electorate;
		electorate.candidateCount = _wtoi(readSingle(in).c_str());
		electorate.candidates = new MMPCandidate[electorate.candidateCount];
		electorate.votes = new vector<MMPVote>;

		electorate.properties = readHeaderTuples(in);

		vector<String>* candidateHeaders = readCSVLine(in);

		int partyHeaderIndex = getIndexOf(candidateHeaders, L"Party");

		for (int j = 0; j < electorate.candidateCount; j++)
		{
			MMPCandidate candidate;

			vector<String>* candidateData = readCSVLine(in);

			candidate.properties = getHeaderTuples(candidateHeaders, candidateData);
			candidate.partyIndex = _wtoi(candidateData->at(partyHeaderIndex).c_str());
			candidate.votes = 0;
			candidate.isExcluded = false;

			electorate.candidates[j] = candidate;

			delete candidateData;
		}

		delete candidateHeaders;

		this->electorates[i] = electorate;

		int numberOfVotesInElectorate = _wtoi(readSingle(in).c_str());

		vector<String>* voteHeaders = readCSVLine(in);
		int localVoteHeaderIndex = getIndexOf(voteHeaders, L"LocalVote");
		int partyVoteHeaderIndex = getIndexOf(voteHeaders, L"PartyVote");

		for (int k = 0; k < numberOfVotesInElectorate; k++)
		{
			vector<String>* voteData = readCSVLine(in);

			MMPVote vote;

			vote.localVote = _wtoi(voteData->at(localVoteHeaderIndex).c_str());
			vote.partyVote = _wtoi(voteData->at(partyVoteHeaderIndex).c_str());

			electorate.votes->push_back(vote);

			delete voteData;
		}

		delete voteHeaders;
	}

	return 0;
}

int MMPVotingSystem::PrintResults()
{
	wcout << "Election type: MMP" << endl;
	wcout << "Electorate count: " << this->electorateCount << endl;
	wcout << "Seat count: " << this->seatCount << endl;

	int totalValidPartyVotes = 0;

	for (int i = 0; i < this->electorateCount; i++)
	{
		MMPElectorate electorate = this->electorates[i];

		wcout << "Electorate " << i << endl;
		
		int majorityCutoff = electorate.votes->size() / 2;

		int winningIndex = 0;
		int winningVoteCount = 0;

		// Reset vote count for electorate.
		for (int j = 0; j < electorate.candidateCount; j++)
		{
			electorate.candidates[j].votes = 0;
		}

		// Count this round's votes.
		for (int j = 0; j < electorate.votes->size(); j++)
		{
			MMPVote vote = electorate.votes->at(j);

			// Check if the current vote is valid in this round.
			if (vote.localVote >= 0)
			{
				// If so, count it.
				electorate.candidates[vote.localVote].votes++;
			}

			// Check if the current vote is valid in this round.
			if (vote.partyVote >= 0)
			{
				// If so, count it.
				this->parties->at(vote.partyVote).partyVotes++;
				totalValidPartyVotes++;
			}
		}

		// Calculate winner.

		for (int j = 0; j < electorate.candidateCount; j++)
		{
			if (electorate.candidates[j].votes > winningVoteCount)
			{
				winningIndex = j;
				winningVoteCount = electorate.candidates[j].votes;
			}
		}

		this->parties->at(electorate.candidates[winningIndex].partyIndex).isElected = true;
		this->parties->at(electorate.candidates[winningIndex].partyIndex).electorateSeatWinners->push_back(i);

		wcout << L"Electorate " << electorate.properties->at(L"Electorate") << L" Winner " <<
			electorate.candidates[winningIndex].properties->at(L"Name") << L" " << winningVoteCount << L" votes" << endl;
	}

	int partyVoteThreshold = (int)(this->threshold * (double)totalValidPartyVotes);
	int totalInParliamentPartyVotes = 0;

	for (int i = 0; i < this->parties->size(); i++)
	{
		if (this->parties->at(i).partyVotes >= partyVoteThreshold)
		{
			this->parties->at(i).isElected = true;
		}

		if (this->parties->at(i).isElected)			// Remember parties may have gained entrance via a local seat.
		{
			totalInParliamentPartyVotes += this->parties->at(i).partyVotes;
		}
	}

	int overhangTotal = 0;

	for (int i = 0; i < this->parties->size(); i++)
	{
		if (this->parties->at(i).isElected == true)
		{
			int partyOverhang = this->parties->at(i).electorateSeatWinners->size() - (int)((double)this->parties->at(i).partyVotes * (double)this->seatCount / (double)totalInParliamentPartyVotes);

			if (partyOverhang > 0)
			{
				overhangTotal += partyOverhang;
			}
		}
	}

	int listSeats = this->seatCount - this->electorateCount;

	switch (this->overhang)
	{
		case MMPOptions::AllowOverhang:
		case MMPOptions::SupplementaryMember:
			// Do nothing.
			break;
		case MMPOptions::NoOverhang:
			listSeats -= overhangTotal;
			break;
		default:
			wcout << "Unknown or unimplemented overhang method." << endl;
			return -1;
	}

	this->finalSeatCount = listSeats + this->electorateCount;
	int listSeatCount = this->finalSeatCount - this->electorateCount;

	for (int i = 0; i < this->parties->size(); i++)
	{
		if (this->parties->at(i).isElected == true)
		{
			int partyListSeats;
			int partyOverhang = 0;

			switch (this->overhang)
			{
				case MMPOptions::AllowOverhang:
				case MMPOptions::NoOverhang:
					partyOverhang = this->parties->at(i).electorateSeatWinners->size() -
						(int)((double)this->parties->at(i).partyVotes * (double)this->finalSeatCount / (double)totalInParliamentPartyVotes);

					if (partyOverhang < 0)
					{
						partyListSeats = -partyOverhang;
					}
					break;
				case MMPOptions::SupplementaryMember:
					partyListSeats = (int)((double)this->parties->at(i).partyVotes *
						(double)listSeatCount / (double)totalInParliamentPartyVotes);
					break;
				default:
					wcout << "Unknown or unimplemented overhang method." << endl;
					
					return -1;
			}

			this->parties->at(i).listSeats = partyListSeats;

			wcout << L"Party ";
			wcout << this->parties->at(i).partyName;
			wcout << L" List seats: ";
			wcout << this->parties->at(i).listSeats;
			wcout <<
				L" Local seats: " << this->parties->at(i).electorateSeatWinners->size() << endl;
		}
	}


	//wcout << "Winner " << electorate.candidates[winningIndex].properties->at("Name") << " " << winningVoteCount << " votes" << endl << endl;

	return 0;
}

vector<int>* MMPVotingSystem::WithPreferenceOrders(WithPreferenceOrdersInfo info)
{	
	// Per electorate levels are identical to FPP in MMP, SM, etc.
	FPPVotingSystem fpp(FPPOptions::FirstPastThePost);
	
	return fpp.WithPreferenceOrders(info);
}

vector<int>* MMPVotingSystem::CalculateAdditional(vector<int>* currentSeatCount, vector<double>* nationalSupport)
{
	int currentElectorateSeats = 0;

	for (int i = 0; i < currentSeatCount->size(); i++)
		currentElectorateSeats += currentSeatCount->at(i);

	if (this->seatCount == 0)
		this->seatCount = this->electorateCount / this->smAmount;

	int additionalSeats = this->seatCount - this->electorateCount;
	double additionalProportion = (double)additionalSeats / (double)this->seatCount;

	vector<int>* result = new vector<int>();

	for (int i = 0; i < currentSeatCount->size(); i++)
	{
		int alreadyHave = currentSeatCount->at(i);
		double p = nationalSupport->at(i);

		int x;
		
		switch (this->overhang)
		{
			case MMPOptions::Proportional:
				// Ignore what seats are already "won".
				// Calcualte how many are needed and subtract what they already have.
				x = (int)(p * (double)this->seatCount) - alreadyHave;
				break;
			case MMPOptions::SupplementaryMember:
				x = (int)(p * additionalSeats);
				break;
			default:
				return NULL;
		}
		
		result->push_back(x);
	}

	return result;
}