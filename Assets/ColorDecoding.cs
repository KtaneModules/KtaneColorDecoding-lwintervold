using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ColorDecodingHelper;
using System.Text.RegularExpressions;
using System;

public class ColorDecoding : MonoBehaviour {

	public KMBombInfo BombInfo;
	public KMBombModule BombModule;
	public KMAudio Audio;
	public KMSelectable[] InputButtons;
	public GameObject[] IndicatorGrid;
	public GameObject[] DisplayGrid;
	public GameObject[] StageIndicators;

	private static readonly Dictionary<_colors, Color32> _colormap= new Dictionary<_colors, Color32> { 
		{_colors.R, new Color32(0xFF, 0x00, 0x00, 0xFF)},
		{_colors.G, new Color32(0x00, 0x57, 0x00, 0xFF)}, 
		{_colors.B, new Color32(0x0F, 0x00, 0xA1, 0xFF)}, 
		{_colors.Y, new Color32(0xDA, 0xEA, 0x00, 0xFF)},
        {_colors.P, new Color32(0x7C, 0x11, 0x9A, 0xFF)}};

	private static readonly List<List<Constraint>> constraint_tables = new List<List<Constraint>> { 
		new List<Constraint>{
			new Sequence_Exact_Constraint (new List<_colors>{_colors.B, _colors.G, _colors.B}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.B, _colors.B, _colors.Y}),
			new None_Constraint (new List<_colors>{ _colors.R }),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.Y, _colors.P, _colors.G}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.Y, _colors.G, _colors.B})
		}, 
		new List<Constraint>{
			new Sequence_Exact_Constraint (new List<_colors>{_colors.P, _colors.Y, _colors.P}),
			new None_Constraint (new List<_colors>{ _colors.G }),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.Y, _colors.Y, _colors.R}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.R, _colors.P, _colors.Y}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.B, _colors.P, _colors.R})
		},
		new List<Constraint>{
			new Sequence_Exact_Constraint (new List<_colors>{_colors.B, _colors.P, _colors.Y}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.P, _colors.P, _colors.B}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.P, _colors.R, _colors.P}),
			new None_Constraint (new List<_colors>{_colors.G }),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.R, _colors.B, _colors.R})
		},
		new List<Constraint>{
			new Sequence_Exact_Constraint (new List<_colors>{_colors.G, _colors.G, _colors.B}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.Y, _colors.R, _colors.G}),
			new None_Constraint (new List<_colors>{_colors.P }),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.B, _colors.Y, _colors.B}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.R, _colors.G, _colors.B})
		},
		new List<Constraint>{
			new Sequence_Exact_Constraint (new List<_colors>{_colors.G, _colors.G, _colors.Y}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.R, _colors.G, _colors.G}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.Y, _colors.R, _colors.P}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.P, _colors.R, _colors.R}),
			new None_Constraint (new List<_colors>{_colors.B })
		},
		new List<Constraint>{
			new Sequence_Exact_Constraint (new List<_colors>{_colors.P, _colors.G, _colors.G}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.Y, _colors.R, _colors.R}),
			new None_Constraint (new List<_colors>{_colors.B }),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.Y, _colors.Y, _colors.G}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.Y, _colors.G, _colors.R})
		},
		new List<Constraint>{
			new Sequence_Exact_Constraint (new List<_colors>{_colors.B, _colors.B, _colors.G}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.B, _colors.Y, _colors.G}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.P, _colors.Y, _colors.Y}),
			new None_Constraint (new List<_colors>{_colors.R }),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.Y, _colors.B, _colors.G})
		},
		new List<Constraint>{
			new Sequence_Exact_Constraint (new List<_colors>{_colors.P, _colors.G, _colors.B}),
			new None_Constraint (new List<_colors>{_colors.Y }),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.P, _colors.P, _colors.G}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.B, _colors.R, _colors.G}),
			new Sequence_Exact_Constraint (new List<_colors>{_colors.R, _colors.G, _colors.R})
		} };
	private List<Constraint> chosen_constraints;

	private Display display;
	private Indicator indicator;
	private List<int> valid_indexes;
	private List<int> correctly_pressed_slots_stage;

	private int stagenum = 0;
	private int stageprogression = 0;

	private static int _moduleIdCounter = 1;
	private int _moduleId;

	void Start(){
		_moduleId = _moduleIdCounter++;
		for (int i = 0; i < InputButtons.Count(); i++) {
			int j = i;
			InputButtons [i].OnInteract += delegate() {
				HandleSubmission (j);
				return false;
			};
		}
		correctly_pressed_slots_stage = new List<int> ();
		indicator = new Indicator (BombInfo, stagenum);
		generateStage (indicator);
		updateGrids ();
		logStageInfo ();
	}

	private void logStageInfo(){
		Debug.LogFormat ("[Color Decoding #{0}] Current stage: {1}", _moduleId, stagenum + 1);
		Debug.LogFormat ("[Color Decoding #{0}] Current stage progression: {1}/{2} valid slots selected", _moduleId, stageprogression, valid_indexes.Count);
		List<Constraint> expectedconstraints = new List<Constraint> ();
		string expectedsolutions = "";
		string comma_space = "";
		for (int i = stageprogression; i < valid_indexes.Count; i++) {
			expectedsolutions += comma_space;
			expectedsolutions += constraint_tables [indicator.getTableNum ()] [valid_indexes [stageprogression + i]].getPatternAsString ();
			expectedconstraints.Add (constraint_tables [indicator.getTableNum ()] [valid_indexes [stageprogression + i]]);
			if (i == stageprogression)
				comma_space = ", ";
		}
		string expectedslots = "";
		comma_space = "";
		for (int i = stageprogression; i < valid_indexes.Count; i++) {
			foreach (int key in display.getConstraintHashMap().Keys) {
				if (display.getConstraintHashMap () [key].Equals (constraint_tables [indicator.getTableNum ()] [valid_indexes [stageprogression + i]])) {
					expectedslots += comma_space;
					expectedslots += key;
					break;
				}
			}
			if (i == 0)
				comma_space = ", ";
		}
		Debug.LogFormat("[Color Decoding #{0}] Expected solution: {1}", _moduleId, expectedsolutions);
		Debug.LogFormat("[Color Decoding #{0}] Expected slots: {1}", _moduleId, expectedslots);
		display.debugLogBoard ();
	}
		
	private void generateStage(Indicator indicator){
		Dictionary<string,int> constraint_removal_map = new Dictionary<string,int> {
			{ "A", 0 },
			{ "B", 1 },
			{ "C", 2 },
			{ "D", 3 },
			{ "E", 4 }
		};
		chosen_constraints = new List<Constraint> ();
		chosen_constraints.AddRange (constraint_tables [indicator.getTableNum()]);
		None_Constraint chosen_None_Constraint = null;
		foreach (Constraint c in chosen_constraints) {
			if (c.GetType () == typeof(None_Constraint)) {
				chosen_None_Constraint = (None_Constraint) c;
				break;
			}
		}
		//add confounder lists to constraints to load from the rest of the board which do not violate the None_Constraint
		List<Constraint> valid_confounders = new List<Constraint>();
		for (int i = 0; i < constraint_tables.Count; i++) {
			if (indicator.getTableNum() == i)
				continue;
			List<Constraint> table = constraint_tables [i];
			foreach (Constraint c in table) {
				if (!c.getPattern ().Contains (chosen_None_Constraint.getPattern ()[0]) && c.GetType () != typeof(None_Constraint)) {
					valid_confounders.Add (c);
				}
			}
		}
		valid_confounders.Shuffle ();
		//replace skipped indexes from the constraint table with valid constraints from other tables, add one additional constraint if it exists
		List<int> indexes_to_replace = new List<int>();
		for (int i = 0; i < indicator.getSkipInfo() [stagenum].Count(); i++) {
			indexes_to_replace.Add(constraint_removal_map[indicator.getSkipInfo() [stagenum] [i].ToString ()]);
		}
		valid_indexes = new List<int> ();
		for (int i = 0; i < 5; i++){
			if (!indexes_to_replace.Contains (i))
				valid_indexes.Add (i);
		}
		for (int i = 0; i < indexes_to_replace.Count && i < valid_confounders.Count; i++) {
			chosen_constraints[indexes_to_replace[i]] = valid_confounders[i];
		}
		if (indexes_to_replace.Count < valid_confounders.Count) {
			chosen_constraints.Add (valid_confounders [indexes_to_replace.Count]);
		}
		chosen_constraints.Shuffle ();
		//The None_Constraint must be the last element loaded into the display table.
		List<Constraint> constraints_in_build_order = new List<Constraint> ();
		foreach (Constraint c in chosen_constraints) {
			if (c.GetType() == typeof(None_Constraint)) {
				constraints_in_build_order.Add(c);
				break;
			}
		}
		foreach (Constraint c in chosen_constraints) {
			if (c.GetType() == typeof(Sequence_Exact_Constraint))
				constraints_in_build_order.Add (c);
		}
		display = new Display (constraints_in_build_order);
	}

	void depressButton(int slotnum){
		Vector3 button_pos = InputButtons [slotnum].transform.localPosition;
		button_pos.y = button_pos.y - 0.004f;
		InputButtons [slotnum].transform.localPosition = button_pos;
		correctly_pressed_slots_stage.Add (slotnum);
	}

	void resetButtons(){
		Vector3 button_pos;
		for (int i = 0; i < correctly_pressed_slots_stage.Count; i++) {
			button_pos = InputButtons [correctly_pressed_slots_stage [i]].transform.localPosition;
			button_pos.y = button_pos.y + 0.004f;
			InputButtons [correctly_pressed_slots_stage [i]].transform.localPosition = button_pos;
		}
		correctly_pressed_slots_stage = new List<int> ();
	}

	void HandleSubmission(int slotnum){
		if (stagenum == 3)
			return;
		for (int i = 0; i < correctly_pressed_slots_stage.Count; i++) {
			if (correctly_pressed_slots_stage [i] == slotnum)
				return;
		}
		Audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, InputButtons[slotnum].transform);
		InputButtons[slotnum].AddInteractionPunch (0.2f);
        int correct_slot = valid_indexes[stageprogression];
		display.debugLogBoard ();
		Debug.LogFormat ("[Color Decoding #{0}] Expected slot: {1}. Entered slot {2}.", _moduleId, correct_slot , slotnum);
		if (display.getConstraintHashMap().ContainsKey(slotnum) && display.getConstraintHashMap()[slotnum].Equals(constraint_tables[indicator.getTableNum()][valid_indexes[stageprogression]])) {
			stageprogression++;
			Debug.LogFormat ("[Color Decoding #{0}] Entered correct slot. Completed {1}/{2} submissions for stage {3}.", _moduleId, stageprogression, valid_indexes.Count, stagenum + 1);
			depressButton (slotnum);
		} else {
			Debug.LogFormat ("[Color Decoding #{0}] Entered incorrect slot. Completed {1}/{2} submissions for stage {3}.", _moduleId, stageprogression, valid_indexes.Count, stagenum + 1);
			BombModule.HandleStrike ();
		}
		if (stageprogression > valid_indexes.Count - 1) {
			resetButtons ();
			if (stagenum == 2) {
				Audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.CorrectChime, InputButtons[slotnum].transform);
				Debug.LogFormat ("[Color Decoding #{0}] Bomb successfully defused.", _moduleId);
				stagenum++;
				updateGrids ();
				BombModule.HandlePass ();
			} else {
				stageprogression = 0;
				stagenum++;
				indicator = new Indicator(BombInfo, stagenum);
				generateStage (indicator);
				updateGrids();
				logStageInfo ();
			}
		}
	}

	void updateGrids(){
		if (stagenum != 0) {
			Color32 brightgreen = new Color32 (0x00, 0xFF, 0x02, 0xFF);
			StageIndicators [stagenum - 1].GetComponent<MeshRenderer> ().material.color = brightgreen;
		}
		if (stagenum == 3)
			return;
		List<List<Cell>> indicatorboard = indicator.getBoard ();
		List<List<Cell>> displayboard = display.getBoard ();
		for (int row = 0; row < indicatorboard.Count; row++) {
			for (int col = 0; col < indicatorboard [row].Count; col++) {
				IndicatorGrid [row * indicatorboard.Count + col].GetComponent<MeshRenderer>().material.color = _colormap [indicatorboard [row] [col].getColor ()];
			}
		}
		for (int row = 0; row < displayboard.Count; row++) {
			for (int col = 0; col < displayboard[row].Count; col++) {
				DisplayGrid [row * displayboard.Count + col].GetComponent<MeshRenderer>().material.color = _colormap [displayboard [row] [col].getColor ()];
			}
		}
	}
#pragma warning disable 414
    private string TwitchHelpMessage = "Commands are “col1/.../col6/row1/.../row6” or “c1/.../c6/r1/.../r6”. These can be chained, for example: “!{0} row1 r2 col3 c4”";
#pragma warning restore 414

    private IEnumerable<KMSelectable> ProcessTwitchCommand(string command) {
        var pieces = command.ToLowerInvariant().Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var buttons = new List<KMSelectable>();

        for (int i = 0; i < pieces.Length; i++) {
            switch (pieces[i]) {
                case "col1": case "c1": buttons.Add(InputButtons[6]); break;
                case "col2": case "c2": buttons.Add(InputButtons[7]); break;
                case "col3": case "c3": buttons.Add(InputButtons[8]); break;
                case "col4": case "c4": buttons.Add(InputButtons[9]); break;
                case "col5": case "c5": buttons.Add(InputButtons[10]); break;
                case "col6": case "c6": buttons.Add(InputButtons[11]); break;
                case "row1": case "r1": buttons.Add(InputButtons[0]); break;
                case "row2": case "r2": buttons.Add(InputButtons[1]); break;
                case "row3": case "r3": buttons.Add(InputButtons[2]); break;
                case "row4": case "r4": buttons.Add(InputButtons[3]); break;
                case "row5": case "r5": buttons.Add(InputButtons[4]); break;
                case "row6": case "r6": buttons.Add(InputButtons[5]); break;
                default: return null;
            }
        }
        return buttons;
    }
}