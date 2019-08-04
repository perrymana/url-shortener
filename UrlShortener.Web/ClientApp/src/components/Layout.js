import React from 'react';
import { Col, Grid, Row } from 'react-bootstrap';
import NavMenu from './NavMenu';

export default props => (
  <Grid fluid>
    <Row>
      <Col sm={3}>
        <NavMenu />
      </Col>
      <Col sm={9}>
        {props.children}
      </Col>
    </Row>
  </Grid>
);

//export default props => (
//    <Grid fluid>
//        <Row>
//            <h1>Smaller Url</h1>
//        </Row>
//        <Row>
//            <Col>
//                {props.children}
//            </Col>
//        </Row>
//    </Grid>

//);
