#include <string>
#pragma once
using namespace std;

class Character
{
public:
	Character(string name, string race, int lvl);
	~Character();
	string getName();
	int getWalkSpeed();
	void setStats(int hp, int str, int dex, int con, int intell, int wis, int cha);

private:
	string name;
	string race;
	int walkSpeed;
	int level;
	int exp;

	//stats
	int hp;
	int ac;
	int strength;
	int dexterity;
	int constitution;
	int intelligence;
	int wisdom;
	int charisma;

	//saves
	int fortSave;
	int reflexSave;
	int willSave;

	void setWalkSpeed();
};

