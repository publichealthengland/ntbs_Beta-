import Vue from 'vue';
const axios = require('axios');

const ValidateInput = Vue.extend({
    props: ['property'],
    data: function() {
      return {
        hasError: false
      }},
  methods: {
    validate: function (event: FocusEvent) {
        // Our onBlur validate events happen on input fields
        const inputField = event.target as HTMLInputElement
        const newValue = inputField.value;
        var headers = {
            "RequestVerificationToken": (<HTMLInputElement>document.querySelector('[name="__RequestVerificationToken"]')).value
        }

        axios.post(`/Patients/Edit/ValidateProperty?key=${this.$props.property}&value=${newValue}`, null, { headers: headers })
        .then((response: any) => {
            console.log(response);
            var errorMessage = response.data;
            this.hasError = errorMessage != '';
            this.$refs["errorField"].textContent = errorMessage;
          })
        .catch((error: any) => {
            console.log(error.response)
        });
    }
  }
});

export {
    ValidateInput
};