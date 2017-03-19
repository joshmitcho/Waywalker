#include <string>
#pragma once
using namespace std;
class Spell
{
public:
	Spell();
	~Spell();

private:
	int damage;
	string school;
	void secondaryEffects();
};

