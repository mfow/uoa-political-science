#pragma once

class PreferenceOrder
{
public:
	PreferenceOrder(void);
	~PreferenceOrder(void);
	vector<int>* order;
	double weight;
};
