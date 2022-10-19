import React, { useState } from "react";
import SquareIcon from '@mui/icons-material/Square';
import './ReservationTable.css';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';

const Reservations = [
  {
    id: 1,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 10,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 3,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 4,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 5,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 6,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
];

class ReservationTable extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      List: Reservations,
      MasterChecked: false,
      SelectedList: [],
      HeaderArrows: Array(Object.keys(Reservations[0]).length - 1).fill(0),
    };
  }

  // Select/ UnSelect Table rows
  onMasterCheck(e) {
    let tempList = this.state.List;
    // Check/ UnCheck All Items
    tempList.map((user) => (user.selected = e.target.checked));

    // Update State
    this.setState({
      MasterChecked: e.target.checked,
      List: tempList,
      SelectedList: this.state.List.filter((e) => e.selected),
    });
  }

  // Update List Item's state and Master Checkbox State
  onItemCheck(e, item) {
    let tempList = this.state.List;
    tempList.map((reservation) => {
      if (reservation.id === item.id) {
        reservation.selected = e.target.checked;
      }
      return reservation;
    });

    //To Control Master Checkbox State
    const totalItems = this.state.List.length;
    const totalCheckedItems = tempList.filter((e) => e.selected).length;

    // Update State 
    this.setState({
      MasterChecked: totalItems === totalCheckedItems,
      List: tempList,
      SelectedList: this.state.List.filter((e) => e.selected),
    });
  }

  // Event to get selected rows(Optional)
  getSelectedRows() {
    this.setState({
      SelectedList: this.state.List.filter((e) => e.selected),
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
    const tempList = this.state.List;
    const keys = Object.keys(Reservations[0]);
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

    const totalItems = this.state.List.length;
    const totalCheckedItems = tempList.filter((e) => e.selected).length;
    let mastercheck = totalItems === totalCheckedItems;
    if (!mastercheck)
      tempList.map((reservation) => reservation.selected = false);
    this.setState({
      MasterChecked: mastercheck,
      List: tempList
    });
  }

  render() {
    return (
      <div className="container">
        <div className="row">
          <div className="col-md-12">
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
                  arrowAction={() => this.handleHeaderClick(2)}/>
                  <HeaderCellWithHover text="Cognome Tutor" arrowDirection={this.state.HeaderArrows[3]} 
                  arrowAction={() => this.handleHeaderClick(3)}/>
                  <HeaderCellWithHover text="Codice Esame" arrowDirection={this.state.HeaderArrows[4]} 
                  arrowAction={() => this.handleHeaderClick(4)}/>
                  <HeaderCellWithHover text="Cod. Matr. Studente" arrowDirection={this.state.HeaderArrows[5]} 
                  arrowAction={() => this.handleHeaderClick(5)}/>
                  <HeaderCellWithHover text="Data" arrowDirection={this.state.HeaderArrows[6]} 
                  arrowAction={() => this.handleHeaderClick(6)}/>
                  <HeaderCellWithHover text="Stato" arrowDirection={this.state.HeaderArrows[7]} 
                  arrowAction={() => this.handleHeaderClick(7)}/>
                </tr>
              </thead>
              <tbody>
                {this.state.List.map((reservation) => (
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
                ))}
              </tbody>
            </table>
            <button
              className="btn btn-primary"
              onClick={() => this.getSelectedRows()}
            >
              Prenotazioni Selezionate {this.state.SelectedList.length}
            </button>
            <button
              className="btn btn-primary"
              onClick={() => this.sortByReservation()}
            >
              Sort by id {this.state.SelectedList.length}
            </button>
            <div className="row">
              <b>All Row Items:</b>
              <code>{JSON.stringify(this.state.List)}</code>
            </div>
            <div className="row">
              <b>Selected Row Items(Click Button To Get):</b>
              <code>{JSON.stringify(this.state.SelectedList)}</code>
            </div>
          </div>
        </div>
      </div>
    );
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

export default ReservationTable;