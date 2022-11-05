import React from "react";
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import styles from './ModalPopup.module.css';


import MarkEmailReadIcon from '@mui/icons-material/MarkEmailRead';
import CardContent from '@mui/material/CardContent';
import Collapse from '@mui/material/Collapse';
import Typography from '@mui/material/Typography';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
export default class ModalPopup extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      ResList: props.selectedList
    }
  }

  static getDerivedStateFromProps(props, state) {
    return {
      ResList: props.selectedList
    };
  }

  handleConfirm() {
    this.props.handleVisibility();
  }

  renderBody(props) {
    const rows = props.resList;
    if (rows.length !== 0) {
      return (
        <table className={styles.table}>
          <thead>
            <tr>
              <th scope="col">Prenotazione</th >
              <th scope="col">Cod. Matr. Tutor</th >
              <th scope="col">Nome Tutor</th >
              <th scope="col">Cognome Tutor</th >
              <th scope="col">Codice Esame</th >
              <th scope="col">Cod. Matr. Studente</th >
              <th scope="col">Data</th >
            </tr>
          </thead>
          <tbody>
            {rows.map((reservation) =>
            (
              <tr key={reservation.id}>
                <td>{reservation.id}</td>
                <td>{reservation.tutorNumber}</td>
                <td>{reservation.tutorName}</td>
                <td>{reservation.tutorSurname}</td>
                <td>{reservation.examCode}</td>
                <td>{reservation.studentNumber}</td>
                <td>{reservation.timeStamp}</td>
                <MailCell reservation={reservation} />
              </tr>
            )
            )}
          </tbody>
        </table>
      )
    }
    else
      return (<div>Nessuna prenotazione selezionata</div>);
  }

  render() {
    return (
      <Modal show={this.props.show}
        onHide={this.props.handleVisibility}
        backdrop="static"
        dialogClassName={styles.myModal}
      >
        <Modal.Header closeButton>
          <Modal.Title>Conferma prenotazione selezionate</Modal.Title>
        </Modal.Header>
        <Modal.Body >
          <MailTemplate />
          <this.renderBody resList={this.state.ResList} />
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => this.handleConfirm()}>
            Close
          </Button>
        </Modal.Footer>
      </Modal>
    )
  }
}

class MailCell extends React.Component {
  render() {
    return (
      <td className={styles.tdMail}>
        <MarkEmailReadIcon className={styles.btnMail} />
      </td>
    );
  }
}

function MailTemplate() {
  const [expanded, setExpanded] = React.useState(false);

  const handleExpandClick = () => {
    setExpanded(!expanded);
  };
  return (
    <>
    <div>Mostra Modello Email<ExpandMoreIcon
        expand={expanded.toString()}
        onClick={handleExpandClick}
        aria-expanded={expanded}
        aria-label="show more"
      /></div>
      
      <Collapse in={expanded} timeout="auto" unmountOnExit>
        <CardContent>
          <Typography paragraph>Method:</Typography>
          <Typography paragraph>
            Heat 1/2 cup of the broth in a pot until simmering, add saffron and set
            aside for 10 minutes.
          </Typography>
          <Typography paragraph>
            Heat oil in a (14- to 16-inch) paella pan or a large, deep skillet over
            medium-high heat. Add chicken, shrimp and chorizo, and cook, stirring
            occasionally until lightly browned, 6 to 8 minutes. Transfer shrimp to a
            large plate and set aside, leaving chicken and chorizo in the pan. Add
            pimentón, bay leaves, garlic, tomatoes, onion, salt and pepper, and cook,
            stirring often until thickened and fragrant, about 10 minutes. Add
            saffron broth and remaining 4 1/2 cups chicken broth; bring to a boil.
          </Typography>
          <Typography paragraph>
            Add rice and stir very gently to distribute. Top with artichokes and
            peppers, and cook without stirring, until most of the liquid is absorbed,
            15 to 18 minutes. Reduce heat to medium-low, add reserved shrimp and
            mussels, tucking them down into the rice, and cook again without
            stirring, until mussels have opened and rice is just tender, 5 to 7
            minutes more. (Discard any mussels that don&apos;t open.)
          </Typography>
          <Typography>
            Set aside off of the heat to let rest for 10 minutes, and then serve.
          </Typography>
        </CardContent>
      </Collapse>
    </>
  );
}

