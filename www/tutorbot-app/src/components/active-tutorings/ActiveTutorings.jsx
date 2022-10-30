import React from 'react';

import { HeaderCellWithHover } from '../reservations/ReservationTable';
import configData from "../../config/config.json";
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import { ShowMoreButton } from '../reservations/ReservationTable';
import "./ActiveTutorings.css"

const TutoringsArray = [
  {
    id: 1,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    start_date: "12-09-12 22:22:22",
    end_date: "12-09-12 22:22:22",
    selected: false
  },
  {
    id: 2,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    start_date: "12-09-12 22:22:22",
    end_date: null,
    selected: false
  },
  {
    id: 3,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: 11999,
    studentNumber: "938354",
    start_date: "12-09-12 22:22:22",
    end_date: "12-09-12 22:22:22",
    selected: false
  }
]

class ActiveTutorings extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      Tutorings: TutoringsArray,
      FilteredTutorList: TutoringsArray,
      MasterChecked: false,
      SelectedList: [],
      HeaderArrows: Array(Object.keys(TutoringsArray[0]).length - 1).fill(0),
      VisibleRows: configData.defaultTableRows,
      IsModalVisible: false
    };
  }

  // Select/ UnSelect Table rows
  onMasterCheck(e) {
    let tempList = this.state.FilteredTutorList;
    // Check/ UnCheck All Items
    tempList.map((user) => (user.selected = e.target.checked));

    // Update State
    this.setState({
      MasterChecked: e.target.checked,
      FilteredTutorList: tempList,
      SelectedList: this.state.FilteredTutorList.filter((e) => e.selected),
    });
  }

  // Update List Item's state and Master Checkbox State
  onItemCheck(e, item) {
    let tempList = this.state.FilteredTutorList;
    tempList.map((tutor) => {
      if (tutor.id === item.id) {
        tutor.selected = e.target.checked;
      }
      return tutor;
    });

    //To Control Master Checkbox State
    const totalItems = this.state.Tutorings.length;
    const totalCheckedItems = tempList.filter((e) => e.selected).length;


    // Update State 
    this.setState({
      MasterChecked: totalItems === totalCheckedItems,
      FilteredTutorList: tempList,
      SelectedList: this.state.FilteredTutorList.filter((e) => e.selected),
    });
  }

  handleHeaderClick(i) {
    const tempList = this.state.HeaderArrows
    switch (tempList[i]) {
      case 0:
        tempList[i] = -1;
        break;
      case 1:
        tempList[i] = -1;
        break;
      case -1:
        tempList[i] = 1;
        break;
      default:
        console.error("Invalid HeaderCell arrow direction: " + tempList[i]);
        return;
    }
    tempList.forEach((index) => { 
      if (i !== index) tempList[index] = 0;
    });
    this.setState({
      HeaderArrows: tempList
    });
    this.sortBy(i);
  }

  comparator(x, y, order) {
    if (x === y) return 0;
    if (order === 1)
      return (x > y) ? 1 : -1;
    if (order === -1)
      return (x < y) ? 1 : -1;
    return 0;
  }

  sortBy(i) {
    const tempList = this.state.FilteredTutorList;
    const keys = Object.keys(this.state.FilteredTutorList[0]);
    switch (keys[i]) {
      case "id":
        tempList.sort((x, y) => this.comparator(x.id, y.id, this.state.HeaderArrows[i]));
        break;
      case "tutorNumber":
        tempList.sort((x, y) => this.comparator(x.tutorNumber, y.tutorNumber, this.state.HeaderArrows[i]));
        break;
      case "tutorName":
        tempList.sort((x, y) => this.comparator(x.tutorName, y.tutorName, this.state.HeaderArrows[i]));
        break;
      case "tutorSurname":
        tempList.sort((x, y) => this.comparator(x.tutorSurname, y.tutorSurname, this.state.HeaderArrows[i]));
        break;
      case "examCode":
        tempList.sort((x, y) => this.comparator(x.examCode, y.examCode, this.state.HeaderArrows[i]));
        break;
      case "studentNumber":
        tempList.sort((x, y) => this.comparator(x.studentNumber, y.studentNumber, this.state.HeaderArrows[i]));
        break;
      case "start_date":
        tempList.sort((x, y) => this.comparator(x.start_date, y.start_date, this.state.HeaderArrows[i]));
        break;
      case "state":
        tempList.sort((x, y) => this.comparator(x.state, y.state, this.state.HeaderArrows[i]));
        break;
      default:
        break;
    }

    const totalItems = this.state.FilteredTutorList.length;
    const totalCheckedItems = tempList.filter((e) => e.selected).length;
    let selectedListTemp = this.state.SelectedList;
    let mastercheck = totalItems === totalCheckedItems;
    if (!mastercheck) {
      tempList.map((tutor) => tutor.selected = false);
      // Can be removed if we want to keep checked items after order change
      selectedListTemp = [];
    }
    this.setState({
      MasterChecked: mastercheck,
      FilteredTutorList: tempList,
      SelectedList: selectedListTemp,
    });
  }

  handleShowMoreClick() {
    let newAmount = this.state.VisibleRows + configData.addonTableRows;
    this.setState({
      VisibleRows: newAmount,
    })
  }

  render() {
    const visibleRows = this.state.FilteredTutorList.slice(0, this.state.VisibleRows);
    return(
      <>
      <div className="content">
        <div className="functionsHeader">
          qua le funzioni :L
        </div>
        <table className="table-tutorings">
              <thead>
                <tr>
                  <th scope="col" className="firstCell">
                    <input
                      type="checkbox"
                      className="form-check-input"
                      checked={this.state.MasterChecked}
                      id="mastercheck"
                      onChange={(e) => this.onMasterCheck(e)}
                    />
                  </th>
                  <HeaderCellWithHover arrowDirection={this.state.HeaderArrows[0]} text="Cod. Matr. Tutor"
                    arrowAction={() => this.handleHeaderClick(0)} />
                  <HeaderCellWithHover text="Nome Tutor" arrowDirection={this.state.HeaderArrows[1]}
                    arrowAction={() => this.handleHeaderClick(1)} />
                  <HeaderCellWithHover text="Cognome Tutor" arrowDirection={this.state.HeaderArrows[2]}
                    arrowAction={() => this.handleHeaderClick(2)} />
                  <HeaderCellWithHover text="Cod. Matr. Studente" arrowDirection={this.state.HeaderArrows[3]}
                    arrowAction={() => this.handleHeaderClick(3)} />
                  <HeaderCellWithHover text="Codice Esame" arrowDirection={this.state.HeaderArrows[4]}
                    arrowAction={() => this.handleHeaderClick(4)} />
                  <HeaderCellWithHover text="Data Inizio" arrowDirection={this.state.HeaderArrows[5]}
                    arrowAction={() => this.handleHeaderClick(5)} />
                  <HeaderCellWithHover text="Data Fine" arrowDirection={this.state.HeaderArrows[6]}
                    arrowAction={() => this.handleHeaderClick(6)} />
                </tr>
              </thead>
              <tbody>
                {visibleRows.map((tutoring) =>
                (
                  
                  <tr key={tutoring.id} className={tutoring.selected ? "selected" : ""}>
                    <th scope="row" className="firstCell">
                      <input
                        type="checkbox"
                        checked={tutoring.selected}
                        className="form-check-input"
                        id="rowcheck{user.id}"
                        onChange={(e) => this.onItemCheck(e, tutoring)}
                      />
                    </th>
                    <td>{tutoring.tutorNumber}</td>
                    <td>{tutoring.tutorName}</td>
                    <td>{tutoring.tutorSurname}</td>
                    <td>{tutoring.studentNumber}</td>
                    <td>{tutoring.examCode}</td>
                    <td>{tutoring.start_date}</td>
                    {tutoring.end_date == null ?
                      <td></td> :
                      <td>{tutoring.end_date}</td>}
                  </tr>
                )
                )}
              </tbody>
            </table>
            <ShowMoreButton onClick={() => this.handleShowMoreClick()}
              visibleRows={this.state.VisibleRows}
              maximumRows={this.state.FilteredTutorList.length}
            />
            </div>
            {/* <this.renderDebug visibleList={this.state.FilteredTutorList} selectedList={this.state.SelectedList} /> */}
      </>
    )
  }
}

export default ActiveTutorings;