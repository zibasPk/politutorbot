import React, { useState, Component } from "react";
import styles from './Table.module.css';


import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import TableModal from "./TableModal";


import configData from "../../config/config.json";

/**
 * The Table component will return a table with selection, sorting and search functions and a personalizable modal popup that will receive 
 * the selected rows. The displayed columns are those in the headers prop, only the elements in the content prop with the same keys of the header prop will be displayed as rows. 
 */
class Table extends Component
{
  constructor(props)
  {
    super(props);
    if (props.hasChecks)
      props.content.forEach(item => item.selected = false);
    this.state = {
      Headers: props.headers,
      Content: props.content,
      FilteredContent: props.content,
      ModalProps: props.modalProps,
      HasChecks: props.hasChecks !== undefined ? props.hasChecks : false,
      MasterChecked: false,
      SelectedContent: [],
      HeaderArrows: Array(Object.keys(props.headers).length).fill(0),
      VisibleRows: configData.defaultTableRows,
      IsModalVisible: false,
      SearchOption: Object.keys(props.headers)[0],
      SearchValue: ""
    };
  }

  static getDerivedStateFromProps(props, state)
  {
    if (props.content !== state.Content)
    {
      //Change in props
      props.content.forEach((elem) => elem.selected = false);

      const tempList = props.content.filter(
        (item) => item[state.SearchOption].toString().toLowerCase().includes(state.SearchValue)
      );
      return {
        Content: props.content,
        FilteredContent: tempList,
        SelectedContent: []
      };
    }
    return null; // No change to state
  }


  render()
  {
    let i = 0;
    const HeaderRow = Object.keys(this.state.Headers).map((key) =>
    {
      let temp = i;
      const cell = <HeaderCellWithHover arrowDirection={this.state.HeaderArrows[temp]}
        text={this.state.Headers[key]}
        arrowAction={() => this.handleHeaderClick(key, temp)}
        key={i}
      />
      i++;
      return cell;
    });

    const SearchOptions = Object.keys(this.state.Headers).map((key, i) =>
    {
      return <option value={key} key={i} > {this.state.Headers[key]}</option>;
    });

    const visibleRows = this.state.FilteredContent.slice(0, this.state.VisibleRows);
    return (
      <>
        {
          this.state.HasChecks ? <TableModal
            selectedContent={this.state.SelectedContent}
            show={this.state.IsModalVisible}
            handleVisibility={() => this.handleModalVisibility()}
            alt="Errore nel caricamento dei contenuti"
            {...this.state.ModalProps}
          /> : <></>
        }
        <div className={styles.tableContent}>
          <div className={styles.functionsHeader}>
            <div className={styles.searchDiv}>
              <label htmlFor="search">
                Ricerca:
                <Form.Select className={styles.searchSelect} onChange={(e) => this.changeSearch(e)}>
                  {SearchOptions}
                </Form.Select>
                <Form.Control className={styles.searchInput} type="text" placeholder="cerca qui" onChange={(e) => this.handleSearch(e)} />
              </label>
              <label htmlFor="search">
                Numero di righe da visualizzare:
                <Form.Control className={styles.inputVisrows} type="text" placeholder={configData.defaultTableRows} onChange={(e) => this.handleVisibleAmountChange(e)} />
              </label>
            </div>
            <div className={styles.buttonDiv}>
              {this.state.HasChecks ?
                <Button
                  variant="warning"
                  className={styles.btnConfirmSelected}
                  onClick={() => this.handleModalVisibility()}
                >
                  {this.state.ModalProps.modalTitle} {this.state.SelectedContent.length}
                </Button> :
                <></>
              }
            </div>
          </div>
          <table className={styles.tableContainer}>
            <thead>
              <tr>
                {
                  this.state.HasChecks ? <th scope="col" className={styles.firstCell}>
                    <input
                      type="checkbox"
                      className="form-check-input"
                      checked={this.state.MasterChecked}
                      id="mastercheck"
                      onChange={(e) => this.onMasterCheck(e)}
                    />
                  </th> :
                    <></>
                }
                {HeaderRow}
              </tr>
            </thead>
            <tbody>
              {
                visibleRows.map((row) =>
                  <this.renderRow row={row}
                    headers={this.state.Headers}
                    hasChecks={this.state.HasChecks}
                    key={row.Id}
                    onChange={(e) => this.onItemCheck(e, row)}
                  />)
              }
            </tbody>
          </table>
          {visibleRows.length === 0 ? <div className={styles.noContentAlert}>Nessun contentuto trovato</div> : <></>}
          <ShowMoreButton onClick={() => this.handleShowMoreClick()}
            visibleRows={this.state.VisibleRows}
            maximumRows={this.state.FilteredContent.length}
          />
        </div>
      </>
    );
  }

  renderRow(props)
  {
    const variableRows = Object.keys(props.headers).map((key, i) =>
    {
      if (props.row[key] instanceof Date)
      {
        return <td key={i}>{props.row[key].toLocaleString()}</td>;
      }

      if (props.row[key] === "OFA")
      {
        return <td key={i} className={styles.tdCentered}>OFA</td>;
      }

      if (Number.isInteger(props.row[key]) && props.row[key].toString().length <= configData.tdMaxLenForCenter)
        return <td key={i} className={styles.tdCentered}>{props.row[key].toLocaleString()}</td>;

      return <td key={i}>{props.row[key].toString()}</td>;
    });

    return (
      <>
        <tr key={props.row.Id} className={props.row.selected ? styles.selected : ""}>
          {props.hasChecks ? <th scope="row" className={styles.firstCell}>
            <input
              type="checkbox"
              checked={props.row.selected}
              className="form-check-input"
              onChange={props.onChange}
            />
          </th> :
            <></>}
          {variableRows}
        </tr>
      </>
    )
  }

  // Select/ UnSelect Table rows
  onMasterCheck(e)
  {
    let tempList = this.state.FilteredContent;
    // Check/ UnCheck All Items
    tempList.map((user) => (user.selected = e.target.checked));

    // Update State
    this.setState({
      MasterChecked: e.target.checked,
      FilteredContent: tempList,
      SelectedContent: this.state.FilteredContent.filter((e) => e.selected),
    });
  }

  // Update List Item's state and Master Checkbox State
  onItemCheck(e, item)
  {
    let tempList = this.state.FilteredContent;
    tempList.map((row) =>
    {
      if (row.Id === item.Id)
      {
        row.selected = e.target.checked;
      }
      return row;
    });

    //To Control Master Checkbox State
    const totalItems = this.state.Content.length;
    const totalCheckedItems = tempList.filter((e) => e.selected).length;


    // Update State 
    this.setState({
      MasterChecked: totalItems === totalCheckedItems,
      FilteredContent: tempList,
      SelectedContent: this.state.FilteredContent.filter((e) => e.selected),
    });
  }

  handleHeaderClick(key, i)
  {
    const arrows = this.state.HeaderArrows;
    switch (arrows[i])
    {
      case 0:
        arrows[i] = -1;
        break;
      case 1:
        arrows[i] = -1;
        break;
      case -1:
        arrows[i] = 1;
        break;
      default:
        console.error("Invalid HeaderCell arrow direction: " + arrows[i]);
        return;
    }
    arrows.forEach((index) =>
    {
      if (i !== index) arrows[index] = 0;
    });
    this.setState({
      HeaderArrows: arrows
    });
    this.sortBy(key, i);
  }

  comparator(x, y, order)
  {
    if (typeof x !== typeof y)
    {
      let xStr = x.toString();
      let yStr = y.toString();
      if(order === 1) 
        return xStr.localeCompare(yStr);
      if(order === -1)
        return yStr.localeCompare(xStr);
    }

    if (x == y) return 0;
    if (order === 1)
      return (x > y) ? 1 : -1;
    if (order === -1)
      return (x < y) ? 1 : -1;
    return 0;
  }

  sortBy(key, i)
  {
    const tempList = this.state.FilteredContent;
    tempList.sort((x, y) => this.comparator(x[key], y[key], this.state.HeaderArrows[i]));

    const totalItems = this.state.FilteredContent.length;
    const totalCheckedItems = tempList.filter((e) => e.selected).length;
    let SelectedContentTemp = this.state.SelectedContent;
    let mastercheck = totalItems === totalCheckedItems;
    if (!mastercheck)
    {
      tempList.map((reservation) => reservation.selected = false);
      // Can be removed if we want to keep checked items after order change
      SelectedContentTemp = [];
    }
    this.setState({
      MasterChecked: mastercheck,
      FilteredContent: tempList,
      SelectedContent: SelectedContentTemp,
    });
  }

  handleShowMoreClick()
  {
    let newAmount = this.state.VisibleRows + configData.addonTableRows;
    this.setState({
      VisibleRows: newAmount,
    })
  }

  handleModalVisibility()
  {
    this.setState({
      IsModalVisible: !this.state.IsModalVisible,
    });
  }

  handleVisibleAmountChange(e)
  {
    let amount = e.target.value;
    const regex = /^[0-9]+$/;
    if (!amount || !regex.test(amount) || amount === 0)
      amount = configData.defaultTableRows;
    this.setState({
      VisibleRows: amount,
    });
  }

  filterContent(value)
  {
    return this.state.Content.filter(
      (item) => item[this.state.SearchOption].toString().toLowerCase().includes(value.toLowerCase())
    );
  }

  handleSearch(event)
  {
    let tempList;
    tempList = this.filterContent(event.target.value.toLowerCase());
    this.setState({
      FilteredContent: tempList,
      SearchValue: event.target.value.toLowerCase()
    })
  }

  changeSearch(event)
  {
    this.setState({
      SearchOption: event.target.value
    })
  }
}

export function HeaderCellWithHover(props)
{
  const [iconStyle, setIconStyle] = useState({ display: 'none' });
  const [style, setStyle] = useState({});
  let arrow;

  if (props.arrowDirection === 1 || props.arrowDirection === 0)
    arrow = <KeyboardArrowDownIcon style={iconStyle} className={styles.arrow} />
  if (props.arrowDirection === -1)
    arrow = <KeyboardArrowUpIcon style={iconStyle} className={styles.arrow} />
  return (
    <th scope="col" style={style} key={props.text}
      onMouseEnter={e =>
      {
        setIconStyle({ display: 'inline'});
        setStyle({ cursor: 'pointer' })
      }}
      onMouseLeave={e =>
      {
        setIconStyle({ display: 'none' })
      }}
      onClick={e =>
      {
        props.arrowAction();
      }}
    >
      {props.text}{arrow}
    </th >)
}

export function ShowMoreButton(props)
{

  if (props.visibleRows >= props.maximumRows)
  {
    return (
      <div
        onClick={() => props.onClick()}
        className={styles.btnShowmore}
        style={{ visibility: 'hidden' }}
      >
        Mostra altri
      </div>
    )
  } else
  {
    return (
      <div
        onClick={() => props.onClick()}
        className={styles.btnShowmore}
        style={{ visibility: 'visible' }}
      >
        Mostra altri
      </div>
    )
  }

}

export default Table;