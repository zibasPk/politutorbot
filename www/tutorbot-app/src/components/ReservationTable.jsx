import React, { useState } from "react";
import SquareIcon from '@mui/icons-material/Square';
import './ReservationTable.css';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import configData from "../config/config.json";

class ReservationTable extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      ResList: props.reservations,
      MasterChecked: false,
      SelectedList: [],
      HeaderArrows: Array(Object.keys(props.reservations[0]).length - 1).fill(0),
      VisibleRows: configData.defaultTableRows,
    };
  }

  // Select/ UnSelect Table rows
  onMasterCheck(e) {
    let tempList = this.state.ResList;
    // Check/ UnCheck All Items
    tempList.map((user) => (user.selected = e.target.checked));

    // Update State
    this.setState({
      MasterChecked: e.target.checked,
      List: tempList,
      SelectedList: this.state.ResList.filter((e) => e.selected),
    });
  }

  // Update List Item's state and Master Checkbox State
  onItemCheck(e, item) {
    let tempList = this.state.ResList;
    console.log(item);
    tempList.map((reservation) => {
      if (reservation.id === item.id) {
        reservation.selected = e.target.checked;
      }
      return reservation;
    });

    //To Control Master Checkbox State
    const totalItems = this.state.ResList.length;
    const totalCheckedItems = tempList.filter((e) => e.selected).length;

    // Update State 
    this.setState({
      MasterChecked: totalItems === totalCheckedItems,
      List: tempList,
      SelectedList: this.state.ResList.filter((e) => e.selected),
    });
  }

  // Event to get selected rows(Optional)
  getSelectedRows() {
    this.setState({
      SelectedList: this.state.ResList.filter((e) => e.selected),
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
    tempList.forEach((arrow, index) => { if (i !== index) arrow = 0 });
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
    const tempList = this.state.ResList;
    const keys = Object.keys(this.state.ResList[0]);
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

    const totalItems = this.state.ResList.length;
    const totalCheckedItems = tempList.filter((e) => e.selected).length;
    let selectedListTemp = this.state.SelectedList;
    let mastercheck = totalItems === totalCheckedItems;
    if (!mastercheck) {
      tempList.map((reservation) => reservation.selected = false);
      selectedListTemp = [];
    }
    this.setState({
      MasterChecked: mastercheck,
      List: tempList,
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
    const visibleReservations = this.state.ResList.slice(0, this.state.VisibleRows);
    return (
      <div className="cont">
        <div className="row">

          <div className="col-md-12">
            <button
              className="btn-confirm"
              onClick={() => this.getSelectedRows()}
            >
              Conferma Prenotazioni Selezionate {this.state.SelectedList.length}
            </button>
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
        {visibleReservations.map((reservation) =>
        (
          <tr key={reservation.id} className={reservation.selected ? "selected" : ""}>
            <th scope="row">
              <input
                type="checkbox"
                checked={reservation.selected}
                className="form-check-input"
                id="rowcheck{user.id}"
                onChange={(e) => this.onItemCheck(e, reservation)}
              />
            </th>
            <td>{reservation.id}</td>
            <td>{reservation.tutorNumber}</td>
            <td>{reservation.tutorName}</td>
            <td>{reservation.tutorSurname}</td>
            <td>{reservation.examCode}</td>
            <td>{reservation.studentNumber}</td>
            <td>{reservation.timeStamp}</td>
            {reservation.state ?
              <td style={{ textAlign: 'center' }}><SquareIcon className="newStatusSquare" /></td> :
              <td></td>}
          </tr>
        )
        )}
      </tbody>
            </table>
            <ShowMoreButton onClick={() => this.handleShowMoreClick()}
              visibleRows={this.state.VisibleRows}
              maximumRows={this.state.ResList.length}
            />
            <this.renderDebug />
          </div>
        </div>
      </div>
    );
  }

  renderDebug() {
    if (configData.debugMode)
      return (
        <>
          <div className="row">
            <b>All Row Items:</b>
            <code>{JSON.stringify(this.state.ResList)}</code>
          </div>
          <div className="row">
            <b>Selected Row Items(Click Button To Get):</b>
            <code>{JSON.stringify(this.state.SelectedList)}</code>
          </div>
        </>
      )
  }
}

function HeaderCellWithHover(props) {
  const [iconStyle, setIconStyle] = useState({ visibility: 'hidden' });
  const [style, setStyle] = useState({});
  let arrow;

  if (props.arrowDirection === 1 || props.arrowDirection === 0)
    arrow = <KeyboardArrowDownIcon style={iconStyle} className="arrow" />
  if (props.arrowDirection === -1)
    arrow = <KeyboardArrowUpIcon style={iconStyle} className="arrow" />
  return (
    <th scope="col" style={style}
      onMouseEnter={e => {
        setIconStyle({ visibility: 'visible' });
        setStyle({ cursor: 'pointer' })
      }}
      onMouseLeave={e => {
        setIconStyle({ visibility: 'hidden' })
      }}
      onClick={e => {
        props.arrowAction();
      }}
    >
      {props.text}{arrow}
    </th >)
}

function ShowMoreButton(props) {

  const [style, setStyle] = useState({});
  if (props.visibleRows >= props.maximumRows) {
    return (
      <div
        onClick={() => props.onClick()}
        className="btn-showmore"
        style={{visibility: 'hidden'}}
      >
        Mostra altri
      </div>
    )
  } else {
    return (
      <div
        onClick={() => props.onClick()}
        className="btn-showmore"
        style={{visibility: 'visible'}}
      >
        Mostra altri
      </div>
    )
  }

}

export default ReservationTable;