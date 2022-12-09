import React from "react";
import styles from './ModalBody.module.css';

import configData from "../../config/config.json"

import MarkEmailReadIcon from '@mui/icons-material/MarkEmailRead';
import CardContent from '@mui/material/CardContent';
import Collapse from '@mui/material/Collapse';
import Typography from '@mui/material/Typography';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import BlockIcon from '@mui/icons-material/Block';
import OverlayTrigger from "react-bootstrap/OverlayTrigger";
import { Tooltip } from "react-bootstrap";

class ModalBody extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      ResList: props.selectedContent,
      AlertText: "",
      IsAlertVisible: false
    }
  }

  confirmReservation(id) {
    fetch(configData.botApiUrl + '/reservations/' + id + '/confirm', {
      method: 'PUT',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => {
    console.log(resp);
      if (!resp.ok)
        return resp.text();
    })
      .then((text) => {
        if (text !== undefined) {
          this.setState({
            AlertText: text,
            IsAlertVisible: true
          })
          return;
        }
        // Hide alert after a positive response
        this.setState({
          IsAlertVisible: false
        })
      })
  }

  render() {
    const AlertDisplay = this.state.IsAlertVisible ? {display:"block"}: {display:"none"};
    return (
      <>
        <MailTemplate />
        <div style={AlertDisplay} className={styles.AlertText}>{this.state.AlertText}</div>
        <this.renderContent resList={this.state.ResList} confirmAction={(id) => this.confirmReservation(id)}/>
      </>
    );
  }

  renderContent(props) {
    const rows = props.resList;

    const renderBody = rows.map((reservation) => {
      return (
        <tr key={reservation.Id}>
          <td >{reservation.Id}</td>
          <td >{reservation.Tutor}</td>
          <td >{reservation.TutorSurname}</td>
          <td >{reservation.TutorName}</td>
          <td >{reservation.Exam}</td>
          <td >{reservation.Student}</td>
          <td >{reservation.ReservationTimestamp.toLocaleString()}</td>
          <MailCell action={() => props.confirmAction(reservation.Id)}/>
          <RefuseCell />

        </tr>
      );
    });

    if (rows.length !== 0) {
      return (
        <table className={styles.table}>
          <thead>
            <tr>
              <th scope="col">Prenotazione</th >
              <th scope="col">Cod. Matr. Tutor</th >
              <th scope="col">Cognome Tutor</th >
              <th scope="col">Nome Tutor</th >
              <th scope="col">Codice Esame</th >
              <th scope="col">Cod. Matr. Studente</th >
              <th scope="col">Data</th >
            </tr>
          </thead>
          <tbody>
            {renderBody}
          </tbody>
        </table>
      )
    }
    else
      return (<div>Nessuna prenotazione selezionata</div>);
  }
}

export default ModalBody;


function RefuseCell() {
  return (
    <td className={styles.tdBorderless}>
      <OverlayTrigger
        placement="right"
        overlay={<Tooltip className={styles.modalTooltip}>Rifiuta prenotazione (dopo invio mail)</Tooltip>}
      >
        <BlockIcon className={styles.btnRefuse} />
      </OverlayTrigger>
    </td>
  );
}


class MailCell extends React.Component {

  render() {
    return (
      <td className={styles.tdBorderless}>
        <OverlayTrigger
          placement="right"
          overlay={<Tooltip className={styles.modalTooltip}>Conferma prenotazione (dopo invio mail)</Tooltip>}
        >
          <MarkEmailReadIcon className={styles.btnMail} onClick={this.props.action} />
        </OverlayTrigger>
      </td>
    );
  }
}

function MailTemplate() {
  const [expanded, setExpanded] = React.useState(false);

  const handleExpandClick = () => {
    setExpanded(!expanded);
  };

  const icon = !expanded ? <ExpandMoreIcon
    expand={expanded.toString()}
    onClick={handleExpandClick}
    aria-expanded={expanded}
    aria-label="show more"
    fontSize='none'
    className={styles.btnExpand}
  /> : <ExpandLessIcon
    expand={expanded.toString()}
    onClick={handleExpandClick}
    aria-expanded={expanded}
    aria-label="show more"
    fontSize='none'
    className={styles.btnExpand}
  />;

  return (
    <>
      <h5>Mostra Modello Email{icon}</h5>

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
            saffron broth and remaining 4 asd1/2 cups chicken broth; bring to a boil.
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

