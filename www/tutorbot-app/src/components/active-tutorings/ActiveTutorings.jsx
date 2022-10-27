import React from 'react';

import { HeaderCellWithHover } from '../reservations/ReservationTable';
import configData from "../../config/config.json";
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import { ShowMoreButton } from '../reservations/ReservationTable';

class ActiveTutorings extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      Tutorings: props.tutorings,
      FilteredTutorList: props.tutorings,
      MasterChecked: false,
      SelectedList: [],
      HeaderArrows: Array(Object.keys(props.tutorings[0]).length - 1).fill(0),
      VisibleRows: configData.defaultTableRows,
      IsModalVisible: false
    };
  }

  // Select/ UnSelect Table rows
  onMasterCheck(e) {
    let tempList = this.state.FilteredResList;
    // Check/ UnCheck All Items
    tempList.map((user) => (user.selected = e.target.checked));

    // Update State
    this.setState({
      MasterChecked: e.target.checked,
      FilteredResList: tempList,
      SelectedList: this.state.FilteredResList.filter((e) => e.selected),
    });
  }

  // Update List Item's state and Master Checkbox State
  onItemCheck(e, item) {
    let tempList = this.state.FilteredResList;
    tempList.map((reservation) => {
      if (reservation.id === item.id) {
        reservation.selected = e.target.checked;
      }
      return reservation;
    });

    //To Control Master Checkbox State
    const totalItems = this.state.Reservations.length;
    const totalCheckedItems = tempList.filter((e) => e.selected).length;


    // Update State 
    this.setState({
      MasterChecked: totalItems === totalCheckedItems,
      FilteredResList: tempList,
      SelectedList: this.state.FilteredResList.filter((e) => e.selected),
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
    const tempList = this.state.FilteredResList;
    const keys = Object.keys(this.state.FilteredResList[0]);
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
      case "timeStamp":
        tempList.sort((x, y) => this.comparator(x.timeStamp, y.timeStamp, this.state.HeaderArrows[i]));
        break;
      case "state":
        tempList.sort((x, y) => this.comparator(x.state, y.state, this.state.HeaderArrows[i]));
        break;
      default:
        break;
    }

    const totalItems = this.state.FilteredResList.length;
    const totalCheckedItems = tempList.filter((e) => e.selected).length;
    let selectedListTemp = this.state.SelectedList;
    let mastercheck = totalItems === totalCheckedItems;
    if (!mastercheck) {
      tempList.map((reservation) => reservation.selected = false);
      // Can be removed if we want to keep checked items after order change
      selectedListTemp = [];
    }
    this.setState({
      MasterChecked: mastercheck,
      FilteredResList: tempList,
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
        <table className="table">
              <thead>
                <tr>
                  <th scope="col">
                    <input
                      type="checkbox"
                      className="form-check-input"
                      checked={this.state.MasterChecked}
                      id="mastercheck"
                      onChange={(e) => this.onMasterCheck(e)}
                    />
                  </th>
                  <HeaderCellWithHover arrowDirection={this.state.HeaderArrows[0]} text="Prenotazione"
                    arrowAction={() => this.handleHeaderClick(0)} />
                  <HeaderCellWithHover text="Cod. Matr. Tutor" arrowDirection={this.state.HeaderArrows[1]}
                    arrowAction={() => this.handleHeaderClick(1)} />
                  <HeaderCellWithHover text="Nome Tutor" arrowDirection={this.state.HeaderArrows[2]}
                    arrowAction={() => this.handleHeaderClick(2)} />
                  <HeaderCellWithHover text="Cognome Tutor" arrowDirection={this.state.HeaderArrows[3]}
                    arrowAction={() => this.handleHeaderClick(3)} />
                  <HeaderCellWithHover text="Codice Esame" arrowDirection={this.state.HeaderArrows[4]}
                    arrowAction={() => this.handleHeaderClick(4)} />
                  <HeaderCellWithHover text="Cod. Matr. Studente" arrowDirection={this.state.HeaderArrows[5]}
                    arrowAction={() => this.handleHeaderClick(5)} />
                  <HeaderCellWithHover text="Data" arrowDirection={this.state.HeaderArrows[6]}
                    arrowAction={() => this.handleHeaderClick(6)} />
                  <HeaderCellWithHover text="Stato" arrowDirection={this.state.HeaderArrows[7]}
                    arrowAction={() => this.handleHeaderClick(7)} />
                </tr>
              </thead>
              <tbody>
                {visibleRows.map((tutoring) =>
                (
                  <tr key={tutoring.id} className={tutoring.selected ? "selected" : ""}>
                    <th scope="row">
                      <input
                        type="checkbox"
                        checked={tutoring.selected}
                        className="form-check-input"
                        id="rowcheck{user.id}"
                        onChange={(e) => this.onItemCheck(e, tutoring)}
                      />
                    </th>
                    <td>{tutoring.id}</td>
                    <td>{tutoring.tutorNumber}</td>
                    <td>{tutoring.tutorName}</td>
                    <td>{tutoring.tutorSurname}</td>
                    <td>{tutoring.examCode}</td>
                    <td>{tutoring.studentNumber}</td>
                    <td>{tutoring.timeStamp}</td>
                    {tutoring.state ?
                      <td style={{ textAlign: 'center' }}><ErrorOutlineIcon className="newStatusIcon" /></td> :
                      <td></td>}
                  </tr>
                )
                )}
              </tbody>
            </table>
            <ShowMoreButton onClick={() => this.handleShowMoreClick()}
              visibleRows={this.state.VisibleRows}
              maximumRows={this.state.FilteredTutorList.length}
            />
            <this.renderDebug visibleList={this.state.FilteredTutorList} selectedList={this.state.SelectedList} />
      </>
    )
  }
}

export default ActiveTutorings;