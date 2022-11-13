import React from 'react';
import styles from './EnabledStudents.module.css';

import Form from 'react-bootstrap/Form';
import HelpIcon from '@mui/icons-material/Help';
import Tooltip from '@mui/material/Tooltip';
import ClickAwayListener from '@mui/material/ClickAwayListener';


const StudentArray = [
  123123,
  123199,
  151111,
  993111,
  993112,
  993113,
  993114,
  993115,
  993116,
  993117,
  993118,
  993119,
  993141,
  193111,
  393111,
  593111,
  793111,
  893111,
  913111
];

export default function EnabledStudents() {
  return (
    <>
      <div className={styles.content}>
        <div className={styles.functionsHeader}>
          <div className={styles.addFunctions}>
            <h1>Abilita studenti</h1>
            <Form.Group controlId="formTextEnable" className="mb-3">
              <Form.Label>Abilita Studente</Form.Label>
              <Form.Control type="text" placeholder="Matr. Studente" />
            </Form.Group>
            <Form.Group controlId="formFileEnable" className="mb-3">
              <Form.Label>Carica File CVS</Form.Label><InfoIcon />
              <Form.Control type="file" />
            </Form.Group>
          </div>
          <div className={styles.removeFunctions}>
            <h1>Rimuovi studenti</h1>
            <Form.Group controlId="formTextEnable" className="mb-3">
              <Form.Label>Rimuovi Studente</Form.Label>
              <Form.Control type="text" placeholder="Matr. Studente" />
            </Form.Group>
            <Form.Group controlId="formFileEnable" className="mb-3">
              <Form.Label>Carica File CVS</Form.Label><InfoIcon />
              <Form.Control type="file" />
            </Form.Group>
          </div>
        </div>
        <StudentList studentArray={StudentArray}/>
      </div>
    </>
  );
}

class StudentList extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      Students: props.studentArray,
      FilteredStudents: props.studentArray
    };
  }

  filterList(event) {
    const tempList = this.state.Students.filter(
      (res) => res.toString().includes(event.target.value)
    );

    this.setState({
      FilteredStudents: tempList,
    })
  }

  render() {
    return (
      <div>
        <h2>Studenti Abilitati</h2>
        <Form.Group controlId="formTextSearch" className="mb-3">
          <Form.Label>Cerca Studente</Form.Label>
          <Form.Control type="text" placeholder="Matr. Studente" onChange={(e) => this.filterList(e)}/>
        </Form.Group>
        <div className={styles.resultAlert}>{this.state.FilteredStudents.length} risultati</div>
        <div className={styles.listContainer}>
          <ul className={styles.studentList}>
            {this.state.FilteredStudents.map((s) => (<li key={s}>{s}</li>))}
          </ul>
        </div>
      </div>
    )
  }
}

function InfoIcon() {
  const [open, setOpen] = React.useState(false);

  const handleTooltipClose = () => {
    setOpen(false);
  };

  const handleTooltipOpen = () => {
    setOpen(true);
  };

  return (
    <ClickAwayListener onClickAway={handleTooltipClose}>

      <Tooltip
        PopperProps={{
          disablePortal: true,
        }}
        onClose={handleTooltipClose}
        open={open}
        disableFocusListener
        disableHoverListener
        disableTouchListener
        title="Caricare un file CVS contente un elenco di codici matricola."
      >
        <HelpIcon className={styles.helpIcon} onClick={handleTooltipOpen} />
      </Tooltip>

    </ClickAwayListener>
  );
}