import Vue from 'vue';
const axios = require('axios');

const ValidateDate = Vue.extend({
    props: ['property'],
    data: function() {
      return {
        hasError: false,
      }},
  methods: {
    validate: function () {
        // Our onBlur validate events happen on input fields
        const dayInput = this.$refs["dayInput"];
        const monthInput = this.$refs["monthInput"];
        const yearInput = this.$refs["yearInput"];

        const dayValue = dayInput.value;
        const monthValue = monthInput.value;
        const yearValue = yearInput.value;

        if (dayValue === '' || monthValue === '' || yearValue === '') {
            return;
        }

        var headers = {
            "RequestVerificationToken": $('input:hidden[name="__RequestVerificationToken"]').val()
        }

        axios.post('/Patients/Edit/ValidateDate?key='+ this.$props.property + '&day=' + dayValue + '&month=' + monthValue + '&year=' + yearValue, 
                null, { headers: headers })
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
    ValidateDate
};