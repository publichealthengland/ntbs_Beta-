import Vue from 'vue';

// Component to validate date order. Compares the members of the "dates" array, in ascending index order.
// Bind dates using v-model, and have bound component emit a datechanged event when date changes.
const DateComparison = Vue.extend({
    data: function() {
      return {
        dates: [],
        dateWarnings: []
      }
    },
    methods: {
        datechanged: function (rank: any) {
            var numberOfDates = this.dates.length;
            var currentIndex = parseInt(rank, 10);
            var currentDate = this.dates[currentIndex];

            if (!currentDate) {
                this.clearWarning(currentIndex, numberOfDates);
                return;
            }

            var lowerDateIndex = currentIndex - 1;
            for (var i = 0; i <= lowerDateIndex; i++) {
                // We clear this so the warning is shown with respect to current date
                this.unsetMatchingWarning(i, currentIndex);
            }
            while (lowerDateIndex >= 0)
            {
                if (!this.dates[lowerDateIndex]) {
                    lowerDateIndex--;
                    continue;
                }

                if (this.dates[lowerDateIndex] > currentDate)
                {
                    this.dateWarnings[currentIndex] = { message: warningMessageEarlier(this.$refs[`date${currentIndex}`].name, this.$refs[`date${lowerDateIndex}`].name), comparedTo: lowerDateIndex };
                    return;
                }
                lowerDateIndex--;
            }

            var higherDateIndex = currentIndex + 1;
            for (var i = higherDateIndex; i < numberOfDates; i++) {
                // We clear this so the warning is shown with respect to current date
                this.unsetMatchingWarning(i, currentIndex);
            }
            while (higherDateIndex < numberOfDates)
            {
                if (!this.dates[higherDateIndex]) {
                    higherDateIndex++;
                    continue;
                }

                if (this.dates[higherDateIndex] < currentDate)
                {
                    this.dateWarnings[currentIndex] = { message: warningMessageLater(this.$refs[`date${currentIndex}`].name, this.$refs[`date${higherDateIndex}`].name), comparedTo: higherDateIndex };
                    return;
                }
                higherDateIndex++;
            }

            this.clearWarning(currentIndex, numberOfDates);
        },
        clearWarning: function(currentIndex: number, numberOfDates: number) {
            if (this.dateWarnings[currentIndex]) {
                this.dateWarnings[currentIndex] = null;
            }
            for (var i = 0; i < numberOfDates; i++) {
                if (i === currentIndex) {
                    continue;
                }
                this.unsetMatchingWarning(i, currentIndex);
            }
        },
        unsetMatchingWarning: function(i: number, currentIndex: number) {
            if (this.dateWarnings[i] && this.dateWarnings[i].comparedTo === currentIndex) {
                this.dateWarnings[i] = null;
            }
        }
    },
});

export {
    DateComparison
};

function warningMessageEarlier(first: string, second: string) {
    return `Warning: ${first} is earlier than ${second}`;
};

function warningMessageLater(first: string, second: string) {
    return `Warning: ${first} is later than ${second}`;
};
