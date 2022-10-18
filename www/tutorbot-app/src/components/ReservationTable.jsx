import React, { useState } from "react";
import SquareIcon from '@mui/icons-material/Square';
import './ReservationTable.css';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';

const Reservations = [
  {
    id: 1,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    email: "Sincere@april.biz",
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
    email: "Sincere@april.biz",
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
    email: "Sincere@april.biz",
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
    email: "Sincere@april.biz",
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
    email: "Sincere@april.biz",
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
    email: "Sincere@april.biz",
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

  sortByNumber() {
    const tempList = this.state.List;
    tempList.sort(function (x, y) {
      if (x.id == y.id) return 0;
      return (x.id > y.id) ? 1 : -1;
    })

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
                  <HeaderCellWithHover text="Prenotazione" />
                  <HeaderCellWithHover text="Cod. Matr. Tutor" />
                  <HeaderCellWithHover text="Nome Tutor" />
                  <HeaderCellWithHover text="Cognome Tutor" />
                  <HeaderCellWithHover text="Codice Esame" />
                  <HeaderCellWithHover text="Cod. Matr. Studente" />
                  <HeaderCellWithHover text="Data" />
                  <HeaderCellWithHover text="Stato" />
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
              onClick={() => this.sortByNumber()}
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
  return (
    <th scope="col" style={style}
      onMouseEnter={e => {
        setIconStyle({ visibility: 'visible'});
        setStyle({cursor: 'pointer'})
      }}
      onMouseLeave={e => {
        setIconStyle({ visibility: 'hidden' })
      }}>
      {props.text}<KeyboardArrowDownIcon style={iconStyle} className="arrow"/>
    </th >)
}

export default ReservationTable;