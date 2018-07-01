#include "StdAfx.h"
#include "DefaultVotePredictor.h"
#include "CSVReader.h"

DefaultVotePredictor::DefaultVotePredictor(void)
{
}


DefaultVotePredictor::~DefaultVotePredictor(void)
{
	delete this->spatialProbability;
}

void DefaultVotePredictor::Load(wifstream &in, int partyCount)
{
	this->partyCount = _wtoi(readSingle(in).c_str());
	this->electorateCount = _wtoi(readSingle(in).c_str());

	if (this->partyCount != partyCount)
	{
		wcout << "WARNING: Inconsistant party counts.";
	}

	this->spatialProbability = new double[this->electorateCount * this->partyCount];
	
	for (int i = 0; i < this->electorateCount; i++)
	{
		vector<String>* line = readCSVLine(in);

		for (int j = 0; j < this->partyCount; j++)
		{
			this->spatialProbability[i * this->partyCount + j] = _wtof(line->at(j).c_str());
		}

		delete line;
	}
}

vector<double>* DefaultVotePredictor::CalculateFirstPreferencesForParties(int electorateIndex, vector<double>* overallSupport)
{
	vector<double>* result = new vector<double>();

	for (int i = 0; i < this->partyCount; i++)
	{
		// Note that with changes in support levels, this algorithm may not return a vector with results that sum to one. (Population changes)
		result->push_back(overallSupport->at(i) * this->spatialProbability[electorateIndex * this->partyCount + i]);
	}

	return result;
}