#include "StdAfx.h"
#include "Simulator.h"

Simulator::Simulator(void)
{
	this->votingSystem = NULL;
	this->predictor = NULL;
	this->prefPredictor = NULL;
	this->partyCount = -1;
	this->partyNames = new vector<String>();
	this->pollResults = new vector<double>();
}


Simulator::~Simulator(void)
{
}

vector<int>* Simulator::Simulate()
{
	int tieCount = 0;
	int* seatCountByParty = new int[this->partyCount];

	for (int i = 0; i < this->partyCount; i++)
		seatCountByParty[i] = 0;

	for (int i = 0; i < this->electorateCount; i++)
	{
		vector<double>* supportLevels = this->predictor->CalculateFirstPreferencesForParties(i, this->pollResults);
		
		vector<PreferenceOrder*>* electoratePrefOrders = new vector<PreferenceOrder*>();

		for (int j = 0; j < this->partyCount; j++)
		{
			vector<PreferenceOrder*>* prefOrders = this->prefPredictor->CalculatePreferenceOrders(j, supportLevels->at(j));

			for (int k = 0; k < prefOrders->size(); k++)
			{
				PreferenceOrder* prefOrder = prefOrders->at(k);

				if (prefOrder->weight > 0.0)
				{
					electoratePrefOrders->push_back(prefOrder);
				}
				else
				{
					delete prefOrder;
				}
			}

			delete prefOrders;
		}

		// Normalize preference scores. (i.e. weights sum to one)
		double weightSum = 0.0;
		for (int j = 0; j < electoratePrefOrders->size(); j++)
			weightSum += electoratePrefOrders->at(j)->weight;

		for (int j = 0; j < electoratePrefOrders->size(); j++)
			electoratePrefOrders->at(j)->weight /= weightSum;

		int expectedWinners = this->electorateSeats->at(i);

		// Calculate winners.
		WithPreferenceOrdersInfo info;

		info.prefOrders = electoratePrefOrders;
		info.partyCount = this->partyCount;
		info.expectedWinnerCount = expectedWinners;

		vector<int>* winners = this->votingSystem->WithPreferenceOrders(info);

		// If there are more elements in "winners" than the expected number of winners (expectedWinners), then there is a tie.

		if (winners == NULL)
			return NULL;

		if (winners->size() < expectedWinners)
			return NULL;

		int winningIndex;

		if (winners->size() == expectedWinners)
		{
			winningIndex = winners->at(0);
		}
		else
		{
			// There was a tie.
			tieCount++;
			wcout << "TIE BREAKING NOT IMPLEMENTED." << endl;

			winningIndex = winners->at(0);
		}

		seatCountByParty[winningIndex]++;

		for (int j = 0; j < electoratePrefOrders->size(); j++)
		{
			delete electoratePrefOrders->at(j);
		}

		delete winners;
		delete electoratePrefOrders;
		delete supportLevels;

		wcout << L".";
	}

	vector<int>* result = new vector<int>();

	for (int i = 0; i < this->partyCount; i++)
		result->push_back(seatCountByParty[i]);

	delete seatCountByParty;

	// Additional seats for balancing... (used in MMP, SM, etc.)
	vector<int>* additionalSeats = this->votingSystem->CalculateAdditional(result, this->pollResults);

	vector<int>* finalResult = new vector<int>();

	for (int i = 0; i < this->partyCount; i++)
		finalResult->push_back(result->at(i) + additionalSeats->at(i));

	delete result;
	delete additionalSeats;

	return finalResult;
}