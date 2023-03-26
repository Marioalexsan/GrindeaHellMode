A mod that makes Secrets of Grindea unfairly hard.

TODOs:
* Implement proper mechanic for registering new StatEnums

Changes in Hell Mode:

* General:
	* [ ] 

* Player:
	* [ ] All shields now take an extra 10% of their ShieldHP as damage on block (5% on PGuard).
	* [ ] Taking damage now adds a stacking Defense Break debuff per hit
		* Each attack adds a 20% defense reduction, capped at 100%. Taking 5 hits effectively turns your armor into wet paper. The exception is DoT attacks (fire, poison, acid, etc.), which do not reduce Defense.
		* If you have negative Defense (Skull Ring, Daisy Armor, Marino's Sword, etc.), the debuff instead increases it by half of the accumulated value.
		* Defense Break starts decaying after 3 seconds, at a rate of 10% / second.
		* Defense Break resets its decay counter when attacked (but not when other sources add to it).
		* Defense Break is displayed as a percentage below your EP Bar.
		* [ ] Using Blink adds 20% to the Defense Break debuff.
		* [ ] Using Barrier or a Health Potion adds 50% to the Defense Break debuff.
	* [ ] EP now has a 1.5 second delay before it starts regenerating after cast.
	* [ ] Health Potions are now less effective if you're not wounded.
		* Your healing efficiency is set to 30% at full HP, 100% at 1 HP, and interpolated between those values.
	* [ ] 
	s
* Enemies:
	* [ ] Rabbys and Slimes (except Orange) can now lunge diagonally.
	* [ ] Bees now adjust their position towards the target while diving.
	* [ ] Bears can now adjust their direction towards the target while charging.
	* [ ] Bloomos now have 100% extra HP and attack 25% faster.
	* [ ] 

* Boss battles in story:
	* [ ] Vilya - bridge edges are permanently on fire.
	* [ ] Giga Slime - arena edges are permanently slimed.
	* [ ] Phaseman - rockets fire every 5 seconds in an X or + pattern at all players.
	* [ ] GUN-D4M - the robot is now a professional boxer and will punch faster.
	* [ ] 