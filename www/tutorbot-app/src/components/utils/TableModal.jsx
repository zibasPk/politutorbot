import React from "react";
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import styles from './Table.module.css';


export default class TableModal extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      Body: props.modalContent,
      AltMessage: props.alt,
      Title: props.modalTitle,
      ConfirmAction: props.onConfirm,
      OnModalEvent: props.onModalEvent
    }
  }

  handleConfirm() {
    if (this.state.ConfirmAction !== undefined)
      this.state.ConfirmAction();
    this.props.handleVisibility();
    if (this.props.onModalEvent !== undefined)
      this.props.onModalEvent();
  }
  
  renderBody(props) {
    if (props.content !== undefined) {
      return (
        <>
          <props.content 
          selectedContent={props.selectedContent} 
          onModalEvent={() => props.onModalEvent()} 
          contentHeaders={props.contentHeaders}
          />
        </>
      )
    }
    else
      return (<div>{props.alt}</div>);
  }

  render() {
    return (
      <Modal show={this.props.show}
        onHide={this.props.handleVisibility}
        backdrop="static"
        dialogClassName={styles.myModal}
      >
        <Modal.Header closeButton>
          <Modal.Title>{this.state.Title}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <this.renderBody
            content={this.state.Body}
            contentHeaders={this.props.contentHeaders}
            alt={this.state.AltMessage}
            selectedContent={this.props.selectedContent}
            onModalEvent={() => this.props.onModalEvent()} />
        </Modal.Body>
        <Modal.Footer>
          <Button variant="warning" onClick={() => this.handleConfirm()}>
            Annulla
          </Button>
          {
            this.state.ConfirmAction !== undefined ?
              <Button variant="secondary" onClick={() => this.handleConfirm()}>
                Conferma
              </Button>
              :
              <></>
          }
        </Modal.Footer>
      </Modal>
    )
  }
}
